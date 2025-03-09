namespace Nzr.Mson.Serializer;

/// <summary>
/// Serializes boolean values
/// </summary>
public class MsonBooleanSerializer : MsonTypeSerializer
{
    /// <inheritdoc/>
    public override Type[] SupportedTypes => [typeof(bool)];

    /// <inheritdoc/>
    public override string Serialize(object? value, MsonSerializerOptions options)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return (bool)value ? "1" : "0";
    }

    /// <inheritdoc/>
    public override object? Deserialize(string value, Type targetType, MsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value == "1";
    }
}
