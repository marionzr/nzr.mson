namespace Nzr.Mson.Tests.TestData;

public class Product : BaseEntity
{
    public string Name { get; set; }

    public string[] Tags { get; set; }

    public ProductCategory Category { get; set; }

    public ProductStatus Status { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public int? Weight { get; set; }

#pragma warning disable CS8618 // Parameterless constructor is required for MSON
    protected Product() { }
#pragma warning restore CS8618


    public Product(ProductCategory category, string name, decimal price, ProductStatus status, string[] tags, DateTime? releaseDate = null, string? description = null, int? weight = null)
    {
        Category = category;
        Name = name;
        Price = price;
        Status = status;
        Tags = tags;
        ReleaseDate = releaseDate;
        Description = description;
        Weight = weight;
    }
}
