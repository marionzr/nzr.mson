namespace Nzr.Mson.Serializer;

/// <summary>
/// Serializes integer values
/// </summary>
public class MsonIntegerSerializer : MsonTypeSerializer
{
    /// <inheritdoc/>
    public override Type[] SupportedTypes => [typeof(sbyte), typeof(byte), typeof(ushort), typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long)];

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

        return int.Parse(value);
    }
}
