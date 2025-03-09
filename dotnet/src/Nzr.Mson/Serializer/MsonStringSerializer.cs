namespace Nzr.Mson.Serializer;

/// <summary>
/// Serializes string values
/// </summary>
public class MsonStringSerializer : MsonTypeSerializer
{
    /// <inheritdoc/>
    public override Type[] SupportedTypes => [typeof(string), typeof(char)];

    // Reserved characters that need escaping
    private static readonly char[] ReservedChars = { '{', '}', '[', ']', ',' };
    private const char EscapeChar = '\\';

    /// <inheritdoc/>
    public override string Serialize(object? value, MsonSerializerOptions options)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var stringValue = value.ToString()!.Trim();

        // Escape the reserved characters
        foreach (var c in ReservedChars)
        {
            stringValue = stringValue.Replace(c.ToString(), $"{EscapeChar}{c}");
        }

        return stringValue;
    }

    /// <inheritdoc/>
    public override object? Deserialize(string value, Type targetType, MsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var result = value.Trim();

        // Unescape the reserved characters
        for (var i = 0; i < result.Length - 1; i++)
        {
            if (result[i] == EscapeChar && Array.IndexOf(ReservedChars, result[i + 1]) >= 0)
            {
                // Remove the escape character
                result = result.Remove(i, 1);
            }
        }

        return result;
    }
}
