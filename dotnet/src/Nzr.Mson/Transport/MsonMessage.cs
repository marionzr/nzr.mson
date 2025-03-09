namespace Nzr.Mson.Transport;

/// <summary>
/// Represents a MSON message
/// </summary>
public class MsonMessage
{
    /// <summary>
    /// Version identifier
    /// </summary>
    public char Version { get; set; }

    /// <summary>
    /// Fragment position (1-based). In case the message is not fragmented, this value is always 1.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Total number of fragments, if the message is fragmented. Otherwise, this value is always 1.
    /// </summary>
    public int TotalFragments { get; set; }

    /// <summary>
    /// Message content, excluding header.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Complete message, including header.
    /// </summary>
    public string FullMessage => $"{Version}{Position}/{TotalFragments}~{Content}";

    /// <summary>
    /// Header of the message.
    /// </summary>
    public string Header => $"{Version}{Position}/{TotalFragments}";

    /// <summary>
    /// Parses a message from a string
    /// </summary>
    public static MsonMessage Parse(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty.");
        }

        var versionChar = message[0];

        // Find the ~ character that separates header from the content.
        var contentStartIndex = message.IndexOf('~');

        if (contentStartIndex < 0)
        {
            throw new FormatException("Invalid message format: missing '~'");
        }

        // Find the / character that separates position from total.
        var slashIndex = message.IndexOf('/');

        if (slashIndex < 0)
        {
            throw new FormatException("Invalid message format: missing '/'");
        }

        // Extract position
        var positionStr = message.Substring(1, slashIndex - 1);
        var position = int.Parse(positionStr);

        // Extract total fragments
        var totalStr = message.Substring(slashIndex + 1, contentStartIndex - (slashIndex + 1));
        var total = int.Parse(totalStr);

        // Extract content
        var content = message.Substring(contentStartIndex + 1);

        return new MsonMessage
        {
            Version = versionChar,
            Position = position,
            TotalFragments = total,
            Content = content
        };
    }
}
