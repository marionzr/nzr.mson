namespace Nzr.Mson.Tests.TestData;

public class Cart : BaseEntity
{
    public Guid StateId { get; set; }

    public Customer? Customer { get; set; }

    public List<Product> Products { get; set; } = [];

    private Cart() { }

    public Cart(Guid stateId)
    {
        StateId = stateId;
    }
}
