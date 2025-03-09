namespace Nzr.Mson.Serializer;

/// <summary>
/// Base class for MSON type serializers
/// </summary>
public abstract class MsonTypeSerializer
{
    /// <summary>
    /// Types that this serializer handles
    /// </summary>
    public abstract Type[] SupportedTypes { get; }

    /// <summary>
    /// Serializes a value to an MSON string
    /// </summary>
    public abstract string Serialize(object? value, MsonSerializerOptions options);

    /// <summary>
    /// Deserializes an MSON string to a value
    /// </summary>
    public abstract object? Deserialize(string value, Type targetType, MsonSerializerOptions options);

    /// <summary>
    /// Determines if this serializer can handle a specified type
    /// </summary>
    public virtual bool CanHandle(Type type)
    {
        // Handle nullable types by examining the underlying type
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        var canHandle = SupportedTypes.Contains(underlyingType) ||
               SupportedTypes.Any(underlyingType.IsSubclassOf);

        return canHandle;
    }
}
