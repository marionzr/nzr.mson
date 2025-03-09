using Bogus;


namespace Nzr.Mson.Tests.TestData;

/// <summary>
/// A fixture generator for creating test data for cart, product and category entities.
/// This class uses the Bogus library to generate fake data for unit testing purposes.
/// </summary>
public class Fixtures
{
    private int _productIdCounter;
    private int _categoryIdCounter;
    private int _cartIdCounter;
    private int _customerIdCounter;
    private readonly int _seed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcmeDbFixturesGenerator"/> class.
    /// A random seed is generated for consistent fake data across the application.
    /// </summary>
    /// <param name="seed">
    /// Setting fixed integer to make the produced test data reproducible
    /// </param>
    public Fixtures(int seed)
    {
        _seed = seed;
        Randomizer.Seed = new Random(_seed);
    }

    /// <summary>
    /// Resets the product and category ID counters to zero.
    /// Call this if the test database is reset.
    /// </summary>
    public void ResetCounters()
    {
        _cartIdCounter = 0;
        _productIdCounter = 0;
        _categoryIdCounter = 0;
        _customerIdCounter = 0;
    }

    public Cart GenerateCart(params Product[] products)
    {
        var faker = new Faker<Cart>()
            .UseSeed(_seed + _cartIdCounter)  // Ensure unique but deterministic fake data for each cart
            .CustomInstantiator(f =>
                new Cart(f.Random.Uuid())
                {
                    Id = _cartIdCounter + 1,
                    Products = [.. products],
                    CreatedAt = DateTimeOffset.Parse("2018-09-03T01:02:03Z", System.Globalization.CultureInfo.InvariantCulture),
                    LastUpdatedAt = DateTimeOffset.Parse("2018-09-03T10:40:06Z", System.Globalization.CultureInfo.InvariantCulture),
                })
            .FinishWith((faker, cart) => ++_cartIdCounter);
        return faker.Generate();
    }

    public Customer GenerateCustomer()
    {
        var faker = new Faker<Customer>()
            .UseSeed(_seed + _customerIdCounter)  // Ensure unique but deterministic fake data for each customer
            .CustomInstantiator(f =>
                new Customer(f.Internet.Email())
                {
                    Id = _cartIdCounter + 1,
                    CreatedAt = DateTimeOffset.Parse("2017-05-03T01:02:03Z", System.Globalization.CultureInfo.InvariantCulture),
                    LastUpdatedAt = DateTimeOffset.Parse("2024-09-13T10:40:06Z", System.Globalization.CultureInfo.InvariantCulture),
                })
            .FinishWith((faker, cart) => ++_cartIdCounter);
        return faker.Generate();
    }

    public Product GenerateProduct(
        ProductCategory productCategory)
    {
        var faker = new Faker<Product>()
            .UseSeed(_seed + _productIdCounter)  // Ensure unique but deterministic fake data for each product
            .CustomInstantiator(f =>
                    new Product(
                        productCategory,
                        f.Commerce.ProductName(),
                        f.Finance.Amount(10, 1000M),
                        f.PickRandom<ProductStatus>(),
                        f.Commerce.Categories(3),
                        DateTime.Parse("2018-08-30T00:00:00Z", System.Globalization.CultureInfo.InvariantCulture), f.Commerce.ProductDescription(),
                        f.Random.Number(1000)
                    )
                    {
                        Id = _productIdCounter + 1,
                        CreatedAt = DateTimeOffset.Parse("2018-09-02T10:20:30Z", System.Globalization.CultureInfo.InvariantCulture),
                        LastUpdatedAt = DateTimeOffset.Parse("2018-09-02T11:12:13Z", System.Globalization.CultureInfo.InvariantCulture),
                    })
            .FinishWith((faker, product) => ++_productIdCounter);

        var product = faker.Generate();

        return product;
    }

    public ProductCategory GenerateProductCategory(
        string? name = null,
        string? description = null)
    {
        var faker = new Faker<ProductCategory>()
            .UseSeed(_seed + _categoryIdCounter)  // Ensure unique but deterministic fake data for each category
            .CustomInstantiator(f =>
                new ProductCategory(
                    name ?? f.Commerce.Categories(1)[0]
                )
                {
                    Id = _categoryIdCounter + 1,
                    CreatedAt = DateTimeOffset.Parse("2018-09-01T00:01:02Z", System.Globalization.CultureInfo.InvariantCulture),
                    LastUpdatedAt = DateTimeOffset.Parse("2018-09-01T10:59:59Z", System.Globalization.CultureInfo.InvariantCulture),
                })
            .FinishWith((faker, category) => ++_categoryIdCounter);

        return faker.Generate();
    }
}
