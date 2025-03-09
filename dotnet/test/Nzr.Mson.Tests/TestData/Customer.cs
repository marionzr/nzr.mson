namespace Nzr.Mson.Tests.TestData;

public class Customer : BaseEntity
{
    public string EmailAddress { get; init; }

#pragma warning disable CS8618 // Parameterless constructor is required for MSON
    private Customer() { }
#pragma warning restore CS8618

    public Customer(string emailAddress)
    {
        EmailAddress = emailAddress;
    }
}
