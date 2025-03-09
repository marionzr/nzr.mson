namespace Nzr.Mson.Schema;

/// <summary>
/// Represents an MSON schema definition
/// </summary>
public class MsonSchema
{
    /// <summary>
    /// The version identifier, usually a single character (1-9, a-z, A-Z)
    /// </summary>
    public char Version { get; set; } = '1';

    /// <summary>
    /// Name of the schema
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Description of the schema
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Root schema field definition
    /// </summary>
    public required MsonFieldDefinition Root { get; set; }

    /// <summary>
    /// Dictionary mapping .NET types to their corresponding field definitions
    /// </summary>
    private readonly Dictionary<Type, MsonFieldDefinition> _typeDefinitions = [];

    /// <summary>
    /// Registers a type with its field definition
    /// </summary>
    public void RegisterType(Type type, MsonFieldDefinition definition)
    {
        _typeDefinitions[type] = definition;
    }

    /// <summary>
    /// Gets a field definition for a type
    /// </summary>
    public MsonFieldDefinition? GetDefinitionForType(Type type)
    {
        if (_typeDefinitions.TryGetValue(type, out var definition))
        {
            return definition;
        }

        return null;
    }
}
