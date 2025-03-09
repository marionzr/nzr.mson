namespace Nzr.Mson.Tests.TestData;

public class BaseEntity
{
    public long Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastUpdatedAt { get; set; }
}
