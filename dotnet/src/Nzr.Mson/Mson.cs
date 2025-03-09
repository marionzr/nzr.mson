using System.Collections;
using System.Reflection;
using System.Text;
using Nzr.Mson.Schema;
using Nzr.Mson.Serializer.Attributes;
using Nzr.Mson.Transport;

namespace Nzr.Mson;

/// <summary>
/// Main serializer for MSON format
/// </summary>
public partial class MsonSerializer
{
    private readonly MsonSerializerOptions _options;
    private readonly MsonFragmentManager _fragmentManager;
    private readonly Dictionary<Type, MsonObjectSerializer> _objectSerializers = [];

    /// <summary>
    /// Creates a new instance of the MSON serializer
    /// </summary>
    public MsonSerializer() : this(CreateEmptySchema())
    {
    }

    /// <summary>
    /// Creates a new instance of the MSON serializer
    /// </summary>
    /// <param name="options">Serialization options</param>
    public MsonSerializer(MsonSerializerOptions options)
    {
        _options = options;
        _fragmentManager = new MsonFragmentManager(_options.MaxMessageLength);
    }


    /// <summary>
    /// Creates a new instance of the MSON serializer
    /// </summary>
    /// <param name="schema">The positional schema</param>
    public MsonSerializer(MsonSchema schema)
    {
        _options = MsonSerializerOptions.CreateDefault(schema);
        _fragmentManager = new MsonFragmentManager(_options.MaxMessageLength);
    }

    /// <summary>
    /// Creates an empty MSON schema.
    /// </summary>
    /// <returns>MsonSchema</returns>
    public static MsonSchema CreateEmptySchema()
    {
        var schema = new MsonSchema()
        {
            Root = new MsonFieldDefinition()
        };

        return schema;
    }

    /// <summary>
    /// Serializes an object to an MSON string
    /// </summary>
    /// <param name="object">Object to serialize</param>
    /// <param name="fragments">The fragments generated, if the full message is larger that the max length.</param>
    /// <returns>MSON formatted string</returns>
    public string Serialize(object? @object, out string[] fragments)
    {
        var schema = _options.Schema ?? throw new InvalidOperationException("Schema is required for MSON serialization.");

        fragments = [];
        var serialized = "{}";

        if (@object != null)
        {
            serialized = SerializeValue(@object, schema.Root);
        }

        // Check if fragmentation is needed
        if (serialized.Length > _options.MaxMessageLength)
        {
            var msonMessageFragments = _fragmentManager.FragmentMessage(schema.Version, serialized);
            fragments = msonMessageFragments.Select(f => f.FullMessage).ToArray();
        }

        // Add schema version and fragment info for non-fragmented messages
        return $"{schema.Version}1/1~{serialized}";
    }

    /// <summary>
    /// Deserializes an MSON string to an object
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="mson">MSON formatted string</param>
    /// <returns>Deserialized object</returns>
    public T? Deserialize<T>(string mson)
    {
        return (T?)Deserialize(mson, typeof(T));
    }

    /// <summary>
    /// Deserializes an MSON string to an object
    /// </summary>
    /// <exception cref="InvalidOperationException">If the mson string is a fragment</exception>
    /// <exception cref="InvalidOperationException">If the mson version is different from the schema to be used in the deserialization.</exception>
    /// <param name="mson">MSON formatted string</param>
    /// <param name="targetType">Target type</param>
    /// <returns>Deserialized object</returns>
    public object? Deserialize(string mson, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(mson))
        {
            throw new ArgumentException("Message cannot be null or empty.");
        }

        var msonMessage = MsonMessage.Parse(mson);
        string fullMessage;

        // Check if this is a fragmented message
        if (msonMessage.TotalFragments > 1)
        {
            // For now, we're only dealing with the first fragment
            // In a real implementation, you'd collect all fragments before processing
            throw new InvalidOperationException("This mson is a fragment and can not be deserialized. Combine the fragments first using MsonFragmentManager and then call this method.");
        }
        else
        {
            fullMessage = msonMessage.Content;
        }

        // Verify schema version
        if (msonMessage.Version != _options.Schema.Version)
        {
            throw new InvalidOperationException($"Schema version mismatch. Expected {_options.Schema.Version}, got {msonMessage.Version}.");
        }

        return DeserializeValue(fullMessage, targetType, _options.Schema.Root);
    }

    /// <summary>
    /// Serializes a value based on its type and field definition
    /// </summary>
    private string SerializeValue(object? value, MsonFieldDefinition? fieldDef)
    {
        var type = fieldDef?.MemberInfo != null ? GetMemberType(fieldDef.MemberInfo) : value?.GetType();

        if (type != null)
        {
            // Handle primitive types
            var typeSerializer = _options.GetSerializerForType(type);

            if (typeSerializer != null)
            {
                return typeSerializer.Serialize(value, _options);
            }

            // Handle arrays and collections
            if (type.IsArray || (value is IEnumerable && value is not string))
            {
                if (value == null)
                {
                    return "[]";
                }

                return SerializeArray((IEnumerable)value, fieldDef?.ArrayItemDefinition);
            }

            // Handle complex objects
            return SerializeObject(value, type);
        }

        return "{}";
    }

    /// <summary>
    /// Gets the type of a member (property or field)
    /// </summary>
    private static Type GetMemberType(MemberInfo memberInfo)
    {
        if (memberInfo is PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType;
        }
        else if (memberInfo is FieldInfo fieldInfo)
        {
            return fieldInfo.FieldType;
        }

        return typeof(object);
    }

    /// <summary>
    /// Serializes an array or collection
    /// </summary>
    private string SerializeArray(IEnumerable items, MsonFieldDefinition? itemDef)
    {
        var builder = new StringBuilder();
        builder.Append('[');
        var first = true;

        foreach (var item in items)
        {
            if (!first)
            {
                builder.Append(',');
            }
            builder.Append(SerializeValue(item, itemDef));
            first = false;
        }

        builder.Append(']');
        return builder.ToString();
    }

    /// <summary>
    /// Serializes a complex object
    /// </summary>
    private string SerializeObject(object? value, Type type)
    {
        if (value == null)
        {
            return "{}";
        }

        // Get or create object serializer
        if (!_objectSerializers.TryGetValue(type, out var serializer))
        {
            serializer = CreateObjectSerializer(type);
            _objectSerializers[type] = serializer;
        }

        return serializer.Serialize(value);
    }

    /// <summary>
    /// Creates an object serializer for a type
    /// </summary>
    private MsonObjectSerializer CreateObjectSerializer(Type type)
    {
        var typeDef = _options.Schema.GetDefinitionForType(type);
        // Auto-discover properties if no explicit definition exists
        typeDef ??= DiscoverTypeDefinition(type);

        return new MsonObjectSerializer(typeDef, this);
    }

    /// <summary>
    /// Discovers type definition from class properties and attributes
    /// </summary>
    private MsonFieldDefinition DiscoverTypeDefinition(Type type)
    {
        var definition = new MsonFieldDefinition
        {
            Description = type.Name,
        };

        // Get public properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite);

        var position = 1;

        foreach (var prop in properties)
        {
            // Skip ignored properties
            if (prop.GetCustomAttribute<MsonIgnoreAttribute>() != null)
            {
                continue;
            }

            var fieldDef = new MsonFieldDefinition
            {
                Description = prop.Name,
                Position = position++,
                MemberInfo = prop
            };

            // Determine field type
            SetFieldType(fieldDef, prop.PropertyType);

            definition.AddField(fieldDef);
        }

        // Sort fields by position
        definition.Fields = definition.Fields.OrderBy(f => f.Position).ToList();

        return definition;
    }

    /// <summary>
    /// Sets the field type for a property
    /// </summary>
    private void SetFieldType(MsonFieldDefinition fieldDef, Type propertyType)
    {
        if (propertyType.IsArray || (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string)))
        {
            // Determine item type for arrays
            var itemType = propertyType.IsArray
                ? propertyType.GetElementType()
                : propertyType.GetGenericArguments().FirstOrDefault() ?? typeof(object);

            if (itemType == null)
            {
                throw new InvalidOperationException($"Unable to determine array item type for {propertyType.Name}.");
            }

            var itemDef = new MsonFieldDefinition { Description = "Item" };
            SetFieldType(itemDef, itemType);
            fieldDef.ArrayItemDefinition = itemDef;
        }
        else
        {
            fieldDef.Fields = DiscoverTypeDefinition(propertyType).Fields;
        }
    }

    /// <summary>
    /// Deserializes a value based on the target type and field definition
    /// </summary>
    private object? DeserializeValue(string value, Type targetType, MsonFieldDefinition fieldDef)
    {
        if (string.IsNullOrEmpty(value) || value == "{}")
        {
            return null;
        }

        // Handle primitive types
        var typeSerializer = _options.GetSerializerForType(targetType);

        if (typeSerializer != null)
        {
            return typeSerializer.Deserialize(value, targetType, _options);
        }

        // Handle arrays
        if (targetType.IsArray || (typeof(IEnumerable).IsAssignableFrom(targetType) && targetType != typeof(string)))
        {
            return DeserializeArray(value, targetType, fieldDef.ArrayItemDefinition ??
                throw new InvalidOperationException($"Array item definition not provider for field {fieldDef}."));
        }

        // Handle complex objects
        return DeserializeObject(value, targetType);
    }

    /// <summary>
    /// Deserializes an array or collection
    /// </summary>
    private object DeserializeArray(string value, Type targetType, MsonFieldDefinition itemDef)
    {
        if (!value.StartsWith('[') || !value.EndsWith(']'))
        {
            throw new FormatException($"Invalid array format on field {itemDef}.");
        }

        // Extract array content
        var content = value.Substring(1, value.Length - 2);
        var items = SplitArrayItems(content);

        // Determine item type
        Type itemType;

        if (targetType.IsArray)
        {
            itemType = targetType.GetElementType() ?? throw new InvalidOperationException($"Unable to determine array item type on field {itemDef}.");
        }
        else
        {
            // For collections, get the generic type argument
            var genericArgs = targetType.GetGenericArguments();
            itemType = genericArgs.Length > 0 ? genericArgs[0] : typeof(object);
        }

        // Create array
        var array = Array.CreateInstance(itemType, items.Count);

        for (var i = 0; i < items.Count; i++)
        {
            array.SetValue(DeserializeValue(items[i], itemType, itemDef), i);
        }

        // If target is array, return directly
        if (targetType.IsArray)
        {
            return array;
        }

        // Convert to collection type
        if (targetType.IsGenericType)
        {
            var listType = typeof(List<>).MakeGenericType(itemType);
            var list = Activator.CreateInstance(listType, true) ?? throw new InvalidOperationException($"Unable to create list instance on field {itemDef}.");

            var addMethod = listType.GetMethod("AddRange") ?? throw new MissingMethodException("AddRange method not found.");
            addMethod.Invoke(list, [array]);

            // If target is List<T>, return directly
            if (targetType == listType)
            {
                return list;
            }

            // Try to convert to specific collection type
            if (targetType.IsInterface)
            {
                return list;
            }

            var constructor = targetType.GetConstructor([typeof(IEnumerable<>).MakeGenericType(itemType)]);

            if (constructor != null)
            {
                return constructor.Invoke(new[] { list });
            }
            else
            {
                throw new MissingMethodException($"Constructor not found on target collection item of type {itemType.Name}.");
            }
        }

        return array;
    }

    /// <summary>
    /// Splits array items respecting nested structures
    /// </summary>
    private static List<string> SplitArrayItems(string content)
    {
        var items = new List<string>();
        if (string.IsNullOrEmpty(content))
        {
            return items;
        }

        var start = 0;
        var brackets = 0;
        var isEscaped = false;

        for (var i = 0; i < content.Length; i++)
        {
            var c = content[i];

            // Check if current character is escaped
            if (isEscaped)
            {
                // Skip this character as it's escaped
                isEscaped = false;
                continue;
            }

            // Check for escape character
            if (c == '\\')
            {
                isEscaped = true;
                continue;
            }

            if (c == '{')
            {
                brackets++;
            }
            else if (c == '}')
            {
                brackets--;
            }
            else if (c == '[')
            {
                brackets++;
            }
            else if (c == ']')
            {
                brackets--;
            }
            else if (c == ',' && brackets == 0)
            {
                // Found a separator at the root level
                items.Add(content.Substring(start, i - start));
                start = i + 1;
            }
        }

        // Add the last item
        if (start < content.Length)
        {
            items.Add(content.Substring(start));
        }

        return items;
    }

    /// <summary>
    /// Deserializes a complex object
    /// </summary>
    private object? DeserializeObject(string value, Type targetType)
    {
        // Get or create object serializer
        if (!_objectSerializers.TryGetValue(targetType, out var serializer))
        {
            serializer = CreateObjectSerializer(targetType);
            _objectSerializers[targetType] = serializer;
        }

        return serializer.Deserialize(value, targetType);
    }
}
