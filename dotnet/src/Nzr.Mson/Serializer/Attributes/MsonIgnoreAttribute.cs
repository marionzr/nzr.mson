namespace Nzr.Mson.Serializer.Attributes;

/// <summary>
/// Indicates that a property should be ignored during serialization
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class MsonIgnoreAttribute : Attribute
{
}
