using Nzr.Mson.Serializer.Attributes;

namespace Nzr.Mson.Tests.TestData;


public class ProductCategory : BaseEntity
{
    public string Name { get; set; }


#pragma warning disable CS8618 // Parameterless constructor is required for MSON
    private ProductCategory() { }
#pragma warning restore CS8618

    public ProductCategory(string name)
    {
        Name = name;
    }

    [MsonIgnore]
    public Product[] products { get; set; } = [];
}
