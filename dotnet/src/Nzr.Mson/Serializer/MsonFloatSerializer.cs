namespace Nzr.Mson.Serializer;

/// <summary>
/// Serializes double values
/// </summary>
public class MsonFloatSerializer : MsonTypeSerializer
{
    /// <inheritdoc/>
    public override Type[] SupportedTypes => [typeof(double), typeof(decimal), typeof(float)];

    /// <inheritdoc/>
    public override string Serialize(object? value, MsonSerializerOptions options)
    {
        if (value == null)
        {
            return string.Empty;
        }
        else if (value is decimal @decimal)
        {
            return @decimal.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        else if (value is float @float)
        {
            return @float.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return ((double)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <inheritdoc/>
    public override object? Deserialize(string value, Type targetType, MsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        else if (targetType == typeof(decimal))
        {
            return decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        else if (targetType == typeof(float))
        {
            return float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
    }
}
