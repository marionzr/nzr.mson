using System.Reflection;
using System.Text;
using Nzr.Mson.Schema;

namespace Nzr.Mson;

public partial class MsonSerializer
{
    /// <summary>
    /// Internal serializer for complex objects
    /// </summary>
    public class MsonObjectSerializer
    {
        private readonly MsonFieldDefinition _definition;
        private readonly MsonSerializer _parent;

        /// <summary>
        /// Creates a new instance of <see cref="MsonObjectSerializer"/>
        /// </summary>
        /// <param name="definition">The field definition</param>
        /// <param name="parent">The parent serializer</param>
        /// <exception cref="MissingMemberException"></exception>
        public MsonObjectSerializer(MsonFieldDefinition definition, MsonSerializer parent)
        {
            _definition = definition;
            _parent = parent;
        }

        /// <summary>
        /// Serializes an object to MSON format
        /// </summary>
        /// <param name="value">Object to serialize</param>
        public string Serialize(object value)
        {
            if (value == null)
            {
                return "{}";
            }

            var builder = new StringBuilder();
            var first = true;

            builder.Append('{');

            foreach (var field in _definition.Fields.OrderBy(f => f.Position))
            {
                if (!first)
                {
                    builder.Append(',');
                }

                // Get property value
                object? propertyValue = null;

                if (field.MemberInfo is PropertyInfo propertyInfo)
                {
                    propertyValue = propertyInfo.GetValue(value);
                }
                else if (field.MemberInfo is FieldInfo fieldInfo)
                {
                    propertyValue = fieldInfo.GetValue(value);
                }

                // Serialize the property value
                builder.Append(_parent.SerializeValue(propertyValue, field));

                first = false;
            }

            builder.Append('}');
            return builder.ToString();
        }

        /// <summary>
        /// Deserializes an MSON string to an object
        /// </summary>
        public object? Deserialize(string msonValue, Type targetType)
        {
            if (string.IsNullOrEmpty(msonValue) || msonValue == "{}")
            {
                return null;
            }

            if (!msonValue.StartsWith('{') || !msonValue.EndsWith('}'))
            {
                throw new FormatException("Invalid object format.");
            }

            // Extract object content
            var content = msonValue.Substring(1, msonValue.Length - 2);
            var propertyValues = SplitObjectProperties(content);

            // Create instance
            var instance = Activator.CreateInstance(targetType, true);

            // Set properties
            var index = 0;
            foreach (var field in _definition.Fields.OrderBy(f => f.Position))
            {
                if (index < propertyValues.Count)
                {
                    var propertyValue = propertyValues[index];

                    // Get target property type
                    var propertyType = GetMemberType(field.MemberInfo ?? throw new MissingMemberException("Member info not found."));

                    // Deserialize property value
                    object? value;

                    if (string.IsNullOrEmpty(propertyValue) || propertyValue == "{}")
                    {
                        value = null;
                    }
                    else
                    {
                        value = _parent.DeserializeValue(propertyValue, propertyType, field);
                    }

                    // Set property value
                    if (field.MemberInfo is PropertyInfo propertyInfo)
                    {
                        propertyInfo.SetValue(instance, value);
                    }
                    else if (field.MemberInfo is FieldInfo fieldInfo)
                    {
                        fieldInfo.SetValue(instance, value);
                    }
                }

                index++;
            }

            return instance;
        }

        /// <summary>
        /// Splits object properties respecting nested structures
        /// </summary>
        private static List<string> SplitObjectProperties(string content)
        {
            var properties = new List<string>();

            if (string.IsNullOrEmpty(content))
            {
                return properties;
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
                    properties.Add(content.Substring(start, i - start));
                    start = i + 1;
                }
            }

            // Add the last property
            if (start < content.Length)
            {
                properties.Add(content.Substring(start));
            }

            return properties;
        }
    }
}
