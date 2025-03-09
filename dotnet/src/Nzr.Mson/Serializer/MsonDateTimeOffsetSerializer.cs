namespace Nzr.Mson.Serializer;

/// <summary>
/// Serializes DateTimeOffset values
/// </summary>
public class MsonDateTimeOffsetSerializer : MsonTypeSerializer
{
    /// <inheritdoc/>
    public override Type[] SupportedTypes => [typeof(DateTimeOffset)];

    /// <inheritdoc/>
    public override string Serialize(object? value, MsonSerializerOptions options)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var dto = (DateTimeOffset)value;
        var serialized = dto.ToString("yyyyMMddHHmmssfff") + GetTimeZoneOffset(dto);

        return serialized;
    }

    /// <inheritdoc/>
    public override object? Deserialize(string value, Type targetType, MsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var dateStr = value.Substring(0, 17);
        var offsetStr = value.Substring(17);

        var dt = DateTime.ParseExact(dateStr, "yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);

        var offset = TimeSpan.Zero;

        if (!string.IsNullOrEmpty(offsetStr))
        {
            var offsetSign = offsetStr[0] == '+' ? 1 : -1;
            var offsetHours = int.Parse(offsetStr.Substring(1, 2));
            var offsetMinutes = int.Parse(offsetStr.Substring(3, 2));
            offset = new TimeSpan(offsetHours, offsetMinutes, 0);

            if (offsetSign < 0)
            {
                offset = offset.Negate();
            }
        }

        return new DateTimeOffset(dt, offset);
    }

    private static string GetTimeZoneOffset(DateTimeOffset dto)
    {
        var offset = dto.Offset;
        var sign = offset.TotalMinutes >= 0 ? "+" : "-";
        var hours = Math.Abs(offset.Hours).ToString("00");
        var minutes = Math.Abs(offset.Minutes).ToString("00");

        return sign + hours + minutes;
    }
}
