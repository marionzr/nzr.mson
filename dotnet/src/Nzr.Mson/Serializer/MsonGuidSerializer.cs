namespace Nzr.Mson.Serializer;

/// <summary>
/// Serializes GUID values
/// </summary>
public class MsonGuidSerializer : MsonTypeSerializer
{
    /// <inheritdoc/>
    public override Type[] SupportedTypes => [typeof(Guid)];

    /// <inheritdoc/>
    public override string Serialize(object? value, MsonSerializerOptions options)
    {
        if (value == null || ((Guid)value) == Guid.Empty)
        {
            return string.Empty;
        }

        return ((Guid)value).ToString("N")!;
    }

    /// <inheritdoc/>
    public override object? Deserialize(string value, Type targetType, MsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return Guid.Parse(value);
    }
}
