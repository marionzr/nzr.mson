using System.Text;

namespace Nzr.Mson.Transport;

/// <summary>
/// Manages message fragmentation and reassembly
/// </summary>
public class MsonFragmentManager
{
    private readonly int _maxFragmentSize;

    /// <summary>
    /// Creates a new instance of the MSON fragment manager
    /// </summary>
    /// <param name="maxFragmentSize"></param>
    public MsonFragmentManager(int maxFragmentSize)
    {
        _maxFragmentSize = maxFragmentSize;
    }

    /// <summary>
    /// Splits a message into fragments
    /// </summary>
    public List<MsonMessage> FragmentMessage(char version, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return [];
        }

        var fragments = CalculateFragments(version, message);
        var result = new List<MsonMessage>();

        var totalFragments = fragments.Count;

        for (var i = 0; i < totalFragments; i++)
        {
            var fragment = fragments[i];
            fragment.Version = version;
            fragment.Position = i + 1;
            fragment.TotalFragments = totalFragments;

            result.Add(fragment);
        }

        return result;
    }

    /// <summary>
    /// Calculates fragments for a message
    /// </summary>
    private List<MsonMessage> CalculateFragments(char version, string message)
    {
        var fragments = new List<MsonMessage>();

        // We need to iteratively determine the number of fragments
        // as the header size depends on the total fragments count

        // Try with a simple estimate first
        var estimatedHeaderSize = 4; // a1/1 format
        var estimatedDataSize = _maxFragmentSize - estimatedHeaderSize;
        var estimatedFragmentCount = (int)Math.Ceiling((double)message.Length / estimatedDataSize);

        // If we have more fragments, the header size increases
        while (true)
        {
            // Calculate header size based on current fragment count estimate
            var headerSize = CalculateHeaderSize(version, 1, estimatedFragmentCount);
            var dataSize = _maxFragmentSize - headerSize;
            var actualFragmentCount = (int)Math.Ceiling((double)message.Length / dataSize);

            // If our estimate matches the actual count, we're done
            if (actualFragmentCount == estimatedFragmentCount)
            {
                // Create the fragments
                var remaining = message.Length;
                var currentPos = 0;

                for (var i = 0; i < actualFragmentCount; i++)
                {
                    var fragmentHeaderSize = CalculateHeaderSize(version, i + 1, actualFragmentCount);
                    var fragmentDataSize = Math.Min(_maxFragmentSize - fragmentHeaderSize, remaining);

                    var fragment = new MsonMessage
                    {
                        Version = version,
                        Position = i + 1,
                        TotalFragments = actualFragmentCount,
                        Content = message.Substring(currentPos, fragmentDataSize)
                    };

                    fragments.Add(fragment);

                    currentPos += fragmentDataSize;
                    remaining -= fragmentDataSize;
                }

                break;
            }

            // Update our estimate
            estimatedFragmentCount = actualFragmentCount;
        }

        return fragments;
    }

    /// <summary>
    /// Calculates the size of a fragment header
    /// </summary>
    /// <param name="version">The version of schema</param>
    /// <param name="position">Fragment position</param>
    /// <param name="total">Total number of fragments</param>
    private static int CalculateHeaderSize(char version, int position, int total)
    {
        // Format: a1/2~
        return $"{version}{position}/{total}~".Length;
    }

    /// <summary>
    /// Reassembles fragments into a complete message
    /// </summary>
    public static string ReassembleMessage(IEnumerable<string> fragments)
    {
        var msonFragments = fragments.Select(MsonMessage.Parse).ToList();

        return ReassembleMessage(msonFragments);
    }

    /// <summary>
    /// Reassembles fragments into a complete message
    /// </summary>
    public static string ReassembleMessage(IEnumerable<MsonMessage> fragments)
    {
        if (fragments == null || !fragments.Any())
        {
            return string.Empty;
        }

        // Sort fragments by position
        var orderedFragments = fragments.OrderBy(f => f.Position).ToList();

        // Verify we have all fragments
        var first = orderedFragments[0];
        var expectedCount = first.TotalFragments;

        if (orderedFragments.Count != expectedCount)
        {
            throw new InvalidOperationException($"Expected {expectedCount} fragments, but got {orderedFragments.Count}.");
        }

        // Verify all fragments have the same version and total
        if (orderedFragments.Any(f => f.Version != first.Version || f.TotalFragments != expectedCount))
        {
            throw new InvalidOperationException("Fragments have inconsistent version or total count.");
        }

        // Concatenate fragments
        var builder = new StringBuilder();
        foreach (var fragment in orderedFragments)
        {
            builder.Append(fragment.Content);
        }

        var content = builder.ToString();

        return $"{first.Version}1/1~{content}";
    }
}
