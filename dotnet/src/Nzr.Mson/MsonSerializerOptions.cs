using Nzr.Mson.Schema;
using Nzr.Mson.Serializer;

namespace Nzr.Mson;

/// <summary>
/// Options for MSON serialization and deserialization
/// </summary>
public class MsonSerializerOptions
{
    /// <summary>
    /// Schema definition
    /// </summary>
    public MsonSchema Schema { get; set; }

    /// <summary>
    /// Maximum message length before fragmentation
    /// </summary>
    public int MaxMessageLength { get; set; } = 1024;

    /// <summary>
    /// Type serializers
    /// </summary>
    public List<MsonTypeSerializer> TypeSerializers { get; set; } = [];

    /// <summary>
    /// Creates a new instance of MsonSerializerOptions
    /// </summary>
    /// <param name="schema">The positional schema.</param>
    public MsonSerializerOptions(MsonSchema schema)
    {
        Schema = schema;
    }

    /// <summary>
    /// Gets a type serializer for a specified type
    /// </summary>
    public MsonTypeSerializer? GetSerializerForType(Type type)
    {
        return TypeSerializers.Find(s => s.CanHandle(type));
    }

    /// <summary>
    /// Creates default serializer options
    /// </summary>
    public static MsonSerializerOptions CreateDefault(MsonSchema schema)
    {
        var options = new MsonSerializerOptions(schema);

        // Add default type serializers
        options.TypeSerializers.Add(new MsonBooleanSerializer());
        options.TypeSerializers.Add(new MsonIntegerSerializer());
        options.TypeSerializers.Add(new MsonFloatSerializer());
        options.TypeSerializers.Add(new MsonStringSerializer());
        options.TypeSerializers.Add(new MsonDateTimeSerializer());
        options.TypeSerializers.Add(new MsonDateTimeOffsetSerializer());
        options.TypeSerializers.Add(new MsonEnumSerializer());
        options.TypeSerializers.Add(new MsonGuidSerializer());

        return options;
    }
}
