namespace Nzr.Mson.Serializer;

/// <summary>
/// Serializes enum values
/// </summary>
public class MsonEnumSerializer : MsonTypeSerializer
{
    /// <inheritdoc/>
    public override Type[] SupportedTypes => [typeof(Enum), typeof(char)];

    /// <inheritdoc/>
    public override bool CanHandle(Type type)
    {
        return type.IsEnum;
    }

    /// <inheritdoc/>
    public override string Serialize(object? value, MsonSerializerOptions options)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return value.ToString()!;
    }

    /// <inheritdoc/>
    public override object? Deserialize(string value, Type targetType, MsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return Enum.Parse(targetType, value);
    }
}
