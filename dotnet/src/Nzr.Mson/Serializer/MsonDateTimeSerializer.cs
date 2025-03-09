namespace Nzr.Mson.Serializer;

/// <summary>
/// Serializes DateTime values
/// </summary>
public class MsonDateTimeSerializer : MsonTypeSerializer
{
    /// <inheritdoc/>
    public override Type[] SupportedTypes => [typeof(DateTime)];

    /// <inheritdoc/>
    public override string Serialize(object? value, MsonSerializerOptions options)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var dt = (DateTime)value;
        var serialized = dt.ToString("yyyyMMddHHmmssfff") + GetTimeZoneOffset(dt);

        return serialized;
    }

    /// <inheritdoc/>
    public override object? Deserialize(string value, Type targetType, MsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 22)
        {
            return null;
        }

        var dateStr = value.Substring(0, 17);
        var offsetStr = value.Substring(17);

        var dt = DateTime.ParseExact(dateStr, "yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);

        if (!string.IsNullOrEmpty(offsetStr))
        {
            var offsetSign = offsetStr[0] == '+' ? 1 : -1;
            var offsetHours = int.Parse(offsetStr.Substring(1, 2));
            var offsetMinutes = int.Parse(offsetStr.Substring(3, 2));
            var offset = new TimeSpan(offsetHours, offsetMinutes, 0);

            // Convert to UTC, then apply the offset
            dt = dt.ToUniversalTime().Add(offsetSign * offset);
        }

        return dt;
    }

    private static string GetTimeZoneOffset(DateTime dt)
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(dt);
        var sign = offset.TotalMinutes >= 0 ? "+" : "-";
        var hours = Math.Abs(offset.Hours).ToString("00");
        var minutes = Math.Abs(offset.Minutes).ToString("00");

        return sign + hours + minutes;
    }
}
