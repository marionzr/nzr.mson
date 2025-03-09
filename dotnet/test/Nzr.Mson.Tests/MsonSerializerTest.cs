using FluentAssertions;
using Nzr.Mson.Schema;
using Nzr.Mson.Serializer.Attributes;
using Nzr.Mson.Tests.TestData;
using Nzr.Mson.Transport;
using Snapshooter;
using Snapshooter.Xunit;

namespace Nzr.Mson.Tests;

public partial class MsonSerializerTest
{
    private readonly Fixtures _fixtures = new(1920);

    [Fact]
    public void Serialize_null_should_serialize_as_mson()
    {
        // Arrange

        var serializer = new MsonSerializer();

        var obj = (Cart?)null;

        // Act

        var serialized = serializer.Serialize(obj, out var fragments);

        // Assert

        serialized.Should().MatchSnapshot();
        fragments.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_without_schema_should_deserialize_mson()
    {
        // Arrange

        var serializer = new MsonSerializer();
        var obj = CreateCart();
        var serialized = "11/1~{9c4483b1a523e7c00293052111033373,{Chet.Shanahan@hotmail.com,1,20170503010203000+0000,20240913104006000+0000},[{Licensed Wooden Bacon,[Grocery,Games,Baby],{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},New,374.90,Description with special chars: ' \",20180830020000000+0200,,123456789,20180902102030000+0000,20180902111213000+0000},{Incredible Granite Hat,[Home,Kids,Games],{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},Active,586.21,,20180830020000000+0200,645,987654321,20180902102030000+0000,20180902111213000+0000},{Refined Soft Bike,[],{Home,20202020,20180901000102000+0000,20180901105959000+0000},Inactive,797.52,Description with reserved chars: \\{\\} \\[\\] \\,,,13,111111111,20180902102030000+0000,20180902111213000+0000}],999999999,20180903010203000+0000,20180903104006000+0000}";

        // Act

        var deserialized = serializer.Deserialize<Cart>(serialized);

        // Assert

        deserialized.Should().BeEquivalentTo(obj);
    }

    [Fact]
    public void Serialize_with_schema_should_serialize_as_mson()
    {
        // Arrange

        var schema = CreateSchema();
        var serializer = new MsonSerializer(schema);
        var obj = CreateCart();

        // Act

        var result = serializer.Serialize(obj, out var fragments);

        // Assert

        result.Should().MatchSnapshot();
        fragments.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_with_schema_should_deserialize_mson()
    {
        // Arrange

        var schema = CreateSchema();
        var serializer = new MsonSerializer(schema);
        var obj = CreateCart();
        var serialized = "11/1~{9c4483b1a523e7c00293052111033373,{1,Chet.Shanahan@hotmail.com,20170503010203000+0000,20240913104006000+0000},[{123456789,Licensed Wooden Bacon,Description with special chars: ' \",New,374.90,[Grocery,Games,Baby],20180830020000000+0200,20180902102030000+0000,20180902111213000+0000,{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},},{987654321,Incredible Granite Hat,,Active,586.21,[Home,Kids,Games],20180830020000000+0200,20180902102030000+0000,20180902111213000+0000,{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},645},{111111111,Refined Soft Bike,Description with reserved chars: \\{\\} \\[\\] \\,,Inactive,797.52,[],,20180902102030000+0000,20180902111213000+0000,{Home,20202020,20180901000102000+0000,20180901105959000+0000},13}],999999999,20180903010203000+0000,20180903104006000+0000}";

        // Act

        var deserialized = serializer.Deserialize<Cart>(serialized);

        // Assert

        deserialized.Should().BeEquivalentTo(obj);
    }

    [Fact]
    public void Serialize_serialized_message_gt_max_message_lenght_should_serialize_as_mson_fragments()
    {
        // Arrange

        var options = MsonSerializerOptions.CreateDefault(MsonSerializer.CreateEmptySchema());
        options.MaxMessageLength = 500;

        var serializer = new MsonSerializer(options);

        var obj = CreateCart();

        // Act

        var result = serializer.Serialize(obj, out var fragments);

        // Assert

        result.Should().MatchSnapshot(SnapshotNameExtension.Create("full_message"));
        fragments.Should().MatchSnapshot(SnapshotNameExtension.Create("fragments"));
    }

    [Fact]
    public void Deserialize_should_deserialize_mson_fragments()
    {
        // Arrange

        var serializer = new MsonSerializer();
        var obj = CreateCart();
        var fragments = new string[] {
            "11/2~{9c4483b1a523e7c00293052111033373,{Chet.Shanahan@hotmail.com,1,20170503010203000+0000,20240913104006000+0000},[{Licensed Wooden Bacon,[Grocery,Games,Baby],{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},New,374.90,Description with special chars: ' \",20180830020000000+0200,,123456789,20180902102030000+0000,20180902111213000+0000},{Incredible Granite Hat,[Home,Kids,Games],{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},Active,586.21,,20180830020000000+0200,",
            "12/2~645,987654321,20180902102030000+0000,20180902111213000+0000},{Refined Soft Bike,[],{Home,20202020,20180901000102000+0000,20180901105959000+0000},Inactive,797.52,Description with reserved chars: \\{\\} \\[\\] \\,,,13,111111111,20180902102030000+0000,20180902111213000+0000}],999999999,20180903010203000+0000,20180903104006000+0000}"
        };
        var serialized = MsonFragmentManager.ReassembleMessage(fragments);

        // Act

        var deserialized = serializer.Deserialize<Cart>(serialized);

        // Assert

        deserialized.Should().BeEquivalentTo(obj);
    }

    [Fact]
    public void Deserialize_with_schema_should_deserialize_mson_even_on_extended_objects()
    {
        // Arrange

        var schema = MsonSerializer.CreateEmptySchema();

        var productDefinition = new MsonFieldDefinition(0);

        // Changed the order of the fields
        productDefinition
            .AddField<ExtendedProduct>(p => p.Id)
            .AddField<ExtendedProduct>(p => p.Name)
            .AddField<ExtendedProduct>(p => p.Description)
            .AddField<ExtendedProduct>(p => p.Status)
            .AddField<ExtendedProduct>(p => p.Price)
            .AddField<ExtendedProduct>(p => p.Tags)
            .AddField<ExtendedProduct>(p => p.ReleaseDate)
            .AddField<ExtendedProduct>(p => p.CreatedAt)
            .AddField<ExtendedProduct>(p => p.LastUpdatedAt)
            .AddField<ExtendedProduct>(p => p.Category)
            .AddField<ExtendedProduct>(p => p.Weight);

        var productCategoryDefinition = new MsonFieldDefinition(1);

        // Inverting the order of the fields
        productCategoryDefinition
            .AddField<ProductCategory>(pc => pc.Id, 1)
            .AddField<ProductCategory>(pc => pc.Name, 0)
            .AddField<ProductCategory>(pc => pc.CreatedAt)
            .AddField<ProductCategory>(pc => pc.LastUpdatedAt);

        schema.RegisterType(typeof(ExtendedProduct), productDefinition);
        schema.RegisterType(typeof(ProductCategory), productCategoryDefinition);

        var serializer = new MsonSerializer(schema);
        var obj = CreateCart().Products[0];
        var serialized = "11/1~{123456789,Licensed Wooden Bacon,Description with special chars: ' \",New,374.90,[Grocery,Games,Baby],20180830020000000+0200,20180902102030000+0000,20180902111213000+0000,{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},}";

        // Act

        var deserialized = serializer.Deserialize<ExtendedProduct>(serialized);

        // Assert

        deserialized.Should().BeEquivalentTo(obj);
    }

    [Fact]
    public void Deserialize_with_schema_should_deserialize_mson_even_on_reduced_objects()
    {
        // Arrange

        var schema = MsonSerializer.CreateEmptySchema();

        var productDefinition = new MsonFieldDefinition(0);

        // Changed the order of the fields
        productDefinition
            .AddField<ReducedProduct>(p => p.Id)
            .AddField<ReducedProduct>(p => p.Name)
            .AddField<ReducedProduct>(p => p.Description)
            .AddField<ReducedProduct>(p => p.Status)
            .AddField<ReducedProduct>(p => p.Price)
            // .AddField<ReducedProduct>(p => p.Tags) This field was removed
            .AddField<ReducedProduct>(p => p.ReleaseDate)
            .AddField<ReducedProduct>(p => p.CreatedAt)
            .AddField<ReducedProduct>(p => p.LastUpdatedAt)
            .AddField<ReducedProduct>(p => p.Category)
            .AddField<ReducedProduct>(p => p.Weight);

        var productCategoryDefinition = new MsonFieldDefinition(1);

        // Inverting the order of the fields
        productCategoryDefinition
            .AddField<ProductCategory>(pc => pc.Id, 1)
            .AddField<ProductCategory>(pc => pc.Name, 0)
            .AddField<ProductCategory>(pc => pc.CreatedAt)
            .AddField<ProductCategory>(pc => pc.LastUpdatedAt);

        schema.RegisterType(typeof(ReducedProduct), productDefinition);
        schema.RegisterType(typeof(ProductCategory), productCategoryDefinition);

        var serializer = new MsonSerializer(schema);
        var obj = CreateCart().Products[0];
        var serialized = "11/1~{123456789,Licensed Wooden Bacon,Description with special chars: ' \",New,374.90,20180830020000000+0200,20180902102030000+0000,20180902111213000+0000,{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},}";

        // Act

        var deserialized = serializer.Deserialize<ReducedProduct>(serialized);

        // Assert

        deserialized.Should().BeEquivalentTo(obj, f => f.Excluding(p => p.Tags));
    }

    [Fact]
    public void Serialize_with_schema_should_serialize_as_mson_with_less_proeperties()
    {
        // Arrange

        var schema = MsonSerializer.CreateEmptySchema();

        var productDefinition = new MsonFieldDefinition(0)
            .AddField<Product>(p => p.Id)
            .AddField<Product>(p => p.Name);

        var productCategoryDefinition = new MsonFieldDefinition(1)
            .AddField<ProductCategory>(pc => pc.Id);

        schema.RegisterType(typeof(Product), productDefinition);
        schema.RegisterType(typeof(ProductCategory), productCategoryDefinition);

        var serializer = new MsonSerializer(schema);
        var obj = CreateCart().Products[0];

        // Act

        var serialized = serializer.Serialize(obj, out var _);

        // Assert

        serialized.Should().MatchSnapshot();
    }

    [Fact]
    public void Deserialize_with_schema_should_deserialize_mson_with_less_proeperties()
    {
        // Arrange

        var schema = MsonSerializer.CreateEmptySchema();

        var productDefinition = new MsonFieldDefinition(0)
            .AddField<Product>(p => p.Id)
            .AddField<Product>(p => p.Name);

        schema.RegisterType(typeof(Product), productDefinition);

        var serializer = new MsonSerializer(schema);
        var obj = CreateCart().Products[0];
        var serialized = "11/1~{123456789,Licensed Wooden Bacon}";

        // Act

        var deserialized = serializer.Deserialize<Product>(serialized);

        // Assert

        deserialized.Should().NotBeNull();
        deserialized.Id.Should().Be(obj.Id);
        deserialized.Name.Should().Be(obj.Name);
        deserialized.Category.Should().BeNull();
    }


    public class ExtendedProduct : Product
    {
        public int NewField { get; set; }

        public ExtendedProduct(int newField, ProductCategory category, string name, decimal price, ProductStatus status, string[] tags, DateTime? releaseDate = null, string? description = null, int? weight = null)
            : base(category, name, price, status, tags, releaseDate, description, weight)
        {
            NewField = newField;
        }

        private ExtendedProduct()
        {

        }
    }

    public class ReducedProduct : Product
    {
        public int NewField { get; set; }

        [MsonIgnore]
        private new string[] Tags { get; set; } = [];

        public ReducedProduct(int newField, ProductCategory category, string name, decimal price, ProductStatus status, DateTime? releaseDate = null, string? description = null, int? weight = null)
            : base(category, name, price, status, [], releaseDate, description, weight)
        {
            NewField = newField;
        }

        private ReducedProduct()
        {

        }
    }

    private static MsonSchema CreateSchema()
    {
        var schema = MsonSerializer.CreateEmptySchema();

        var productDefinition = new MsonFieldDefinition(1)
            // Changed the order of the fields
            .AddField<Product>(p => p.Id)
            .AddField<Product>(p => p.Name)
            .AddField<Product>(p => p.Description)
            .AddField<Product>(p => p.Status)
            .AddField<Product>(p => p.Price)
            .AddField<Product>(p => p.Tags)
            .AddField<Product>(p => p.ReleaseDate)
            .AddField<Product>(p => p.CreatedAt)
            .AddField<Product>(p => p.LastUpdatedAt)
            .AddField<Product>(p => p.Category)
            .AddField<Product>(p => p.Weight);

        var productCategoryDefinition = new MsonFieldDefinition(2)
            // Inverting the order of the fields
            .AddField<ProductCategory>(pc => pc.Id, 1)
            .AddField<ProductCategory>(pc => pc.Name, 0)
            .AddField<ProductCategory>(pc => pc.CreatedAt)
            .AddField<ProductCategory>(pc => pc.LastUpdatedAt);

        var customerDefinition = new MsonFieldDefinition(0)
            .AddField<Customer>(u => u.Id)
            .AddField<Customer>(u => u.EmailAddress)
            .AddField<Customer>(u => u.CreatedAt)
            .AddField<Customer>(u => u.LastUpdatedAt);

        schema.RegisterType(typeof(Customer), customerDefinition);
        schema.RegisterType(typeof(Product), productDefinition);
        schema.RegisterType(typeof(ProductCategory), productCategoryDefinition);
        return schema;
    }


    [Fact]
    public void Serialize_without_schema_should_serialize_as_mson()
    {
        // Arrange

        var serializer = new MsonSerializer();

        var obj = CreateCart();

        // Act

        var serialized = serializer.Serialize(obj, out var fragments);

        // Assert

        serialized.Should().MatchSnapshot();
        fragments.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_null_should_deserialize_mson()
    {
        // Arrange

        var serializer = new MsonSerializer();
        var serialized = "11/1~{}";

        // Act

        var deserialized = serializer.Deserialize<Cart>(serialized);

        // Assert

        deserialized.Should().Be(null);
    }

    [Fact]
    public void Serialize_arrays_should_serialize_as_mson()
    {
        // Arrange

        var serializer = new MsonSerializer();

        var obj = CreateCart().Products.ToArray();

        // Act

        var serialized = serializer.Serialize(obj, out var fragments);

        // Assert

        serialized.Should().MatchSnapshot();
        fragments.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_arrays_should_deserialize_mson_array()
    {
        // Arrange

        var schema = MsonSerializer.CreateEmptySchema();
        var productDefinition = new MsonFieldDefinition(1)
           // Changed the order of the fields
           .AddField<Product>(p => p.Id)
           .AddField<Product>(p => p.Name)
           .AddField<Product>(p => p.Description)
           .AddField<Product>(p => p.Status)
           .AddField<Product>(p => p.Price)
           .AddField<Product>(p => p.Tags)
           .AddField<Product>(p => p.ReleaseDate)
           .AddField<Product>(p => p.CreatedAt)
           .AddField<Product>(p => p.LastUpdatedAt)
           .AddField<Product>(p => p.Category)
           .AddField<Product>(p => p.Weight);

        var productArrayDefinition = new MsonFieldDefinition(0)
        {
            ArrayItemDefinition = productDefinition
        };

        schema.Root.ArrayItemDefinition = productArrayDefinition;

        schema.RegisterType(typeof(Product[]), productArrayDefinition);

        var serializer = new MsonSerializer(schema);

        var obj = CreateCart().Products.ToArray();

        // Act

        var serialized = serializer.Serialize(obj, out var fragments);

        // Assert

        serialized.Should().MatchSnapshot();
        fragments.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deserialize_Should_Throw_FormatException_When_Message_Is_Null_Or_Empty(string? invalidMessage)
    {
        // Arrange

        var serializer = new MsonSerializer();

        // Act

        Action act = () => serializer.Deserialize<Product>(invalidMessage!);

        // Assert

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("1MissingFragmentSeparator~{}")]
    [InlineData("11/1MissingContentSeparator{}")]
    [InlineData("11/aInvalidFragmentCount{}")]
    [InlineData("1a/1InvalidFragmentCount{}")]
    public void Deserialize_Should_Throw_FormatException_When_Message_Has_Invalid_Header(string messageWithInvalidHeader)
    {
        // Arrange

        var serializer = new MsonSerializer();

        // Act

        Action act = () => serializer.Deserialize<Product>(messageWithInvalidHeader);

        // Assert

        act.Should().Throw<FormatException>();
    }

    private Cart CreateCart()
    {
        var category1 = _fixtures.GenerateProductCategory();
        var product1 = _fixtures.GenerateProduct(category1);
        var product2 = _fixtures.GenerateProduct(category1);

        var category2 = _fixtures.GenerateProductCategory();
        var product3 = _fixtures.GenerateProduct(category2);

        var customer1 = _fixtures.GenerateCustomer();

        // Set some properties to null
        product1.Weight = null;
        product2.Description = null;
        product3.ReleaseDate = null;

        // Set some special chars
        product1.Description = "Description with special chars: ' \"";
        product3.Description = "Description with reserved chars: {} [] ,";

        // Make some arrays empty
        product3.Tags = [];


        var cart = _fixtures.GenerateCart(product1, product2, product3);
        cart.Customer = customer1;

        // Set better Ids
        category1.Id = 10101010;
        category2.Id = 20202020;
        product1.Id = 123456789;
        product2.Id = 987654321;
        product3.Id = 111111111;
        cart.Id = 999999999;

        return cart;
    }
}
