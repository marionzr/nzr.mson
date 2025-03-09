using System.Linq.Expressions;
using System.Reflection;

namespace Nzr.Mson.Schema;

/// <summary>
/// Defines a field in an MSON schema.
/// </summary>
public class MsonFieldDefinition
{
    /// <summary>
    /// Name of the field.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Position of the field (1-based).
    /// </summary>
    public int Position { get; set; } = -1;

    /// <summary>
    /// Nested field definitions for object types.
    /// </summary>
    public List<MsonFieldDefinition> Fields { get; internal set; } = [];

    /// <summary>
    /// For array types, defines the type of items in the array.
    /// </summary>
    public MsonFieldDefinition? ArrayItemDefinition { get; set; }

    /// <summary>
    /// Property or field info that this definition is mapped to.
    /// </summary>
    public MemberInfo? MemberInfo { get; set; }

    /// <summary>
    /// Creates a new instance of MsonFieldDefinition.
    /// </summary>
    public MsonFieldDefinition()
    {

    }

    /// <summary>
    /// Creates a new instance of MsonFieldDefinition.
    /// </summary>
    /// <param name="position">The position of the field.</param>
    public MsonFieldDefinition(int position)
    {
        Position = position;
    }

    /// <summary>
    /// Creates a new instance of MsonFieldDefinition
    /// </summary>
    /// <typeparam name="T">The type of the property or field.</typeparam>
    /// <param name="memberName">The name of the property or field.</param>
    /// <returns>MemberInfo</returns>
    public static MemberInfo CreateMemberInfo<T>(string memberName)
    {
        var type = typeof(T);
        return type.GetMember(memberName).FirstOrDefault() ?? throw new MissingMemberException(type.Name, memberName);
    }

    /// <summary>
    /// Creates a new instance of MsonFieldDefinition
    /// </summary>
    /// <typeparam name="T">The type of the property or field.</typeparam>
    /// <param name="memberName">The name of the property or field.</param>
    /// <param name="position">The position of the field.</param>
    /// <param name="arrayItemDefinition">The definition of the items in the array.</param>
    /// <returns>MsonFieldDefinition</returns>
    public static MsonFieldDefinition Create<T>(string memberName, int? position = null, MsonFieldDefinition? arrayItemDefinition = null)
    {
        var memberInfo = CreateMemberInfo<T>(memberName);
        var type = GetMemberType(memberInfo);

        if (type.IsArray && arrayItemDefinition == null)
        {
            arrayItemDefinition = new MsonFieldDefinition()
            {
                Position = 0
            };
        }

        var fieldDefinition = new MsonFieldDefinition
        {
            Description = memberName,
            MemberInfo = memberInfo,
            ArrayItemDefinition = arrayItemDefinition,
        };

        fieldDefinition.Position = position ?? fieldDefinition.Position;

        return fieldDefinition;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"MemberInfo: {MemberInfo?.Name}, Position: {Position}, Description: {Description}";
    }

    /// <summary>
    /// Adds a field to the schema.
    /// </summary>
    /// <param name="fieldDef">The field definition to add.</param>
    public MsonFieldDefinition AddField(MsonFieldDefinition fieldDef)
    {
        if (fieldDef.Position == -1)
        {
            fieldDef.Position = Fields.Count;
        }

        if (Fields.Any(f => f.Position == fieldDef.Position))
        {
            throw new InvalidOperationException($"Field with position {fieldDef.Position} already exists.");
        }

        Fields.Add(fieldDef);

        return this;
    }

    /// <summary>
    /// Creates a new instance of MsonFieldDefinition and adds it to the schema.
    /// </summary>
    /// <typeparam name="T">The type of the property or field.</typeparam>
    /// <param name="memberName">The name of the property or field.</param>
    /// <param name="position">The position of the field.</param>
    /// <param name="arrayItemDefinition">The definition of the items in the array.</param>
    /// <returns>MsonFieldDefinition</returns>
    public MsonFieldDefinition AddField<T>(string memberName, int? position = null, MsonFieldDefinition? arrayItemDefinition = null)
    {
        var fieldDef = Create<T>(memberName, position, arrayItemDefinition);
        AddField(fieldDef);

        return this;
    }

    /// <summary>
    /// Adds a field to the schema using a lambda expression to access the property.
    /// </summary>
    /// <typeparam name="T">The type of the property or field.</typeparam>
    /// <param name="memberExpression">The lambda expression representing the property or field.</param>
    /// <param name="position">The position of the field.</param>
    /// <param name="arrayItemDefinition">The definition of the items in the array.</param>
    /// <returns>The current MsonFieldDefinition instance.</returns>
    public MsonFieldDefinition AddField<T>(Expression<Func<T, object?>> memberExpression, int? position = null, MsonFieldDefinition? arrayItemDefinition = null)
    {
        // Extract the property name from the expression
        var memberName = GetMemberNameFromExpression(memberExpression);

        // Create the field definition using the property name
        var fieldDef = Create<T>(memberName, position, arrayItemDefinition);

        // Add the field definition to the current schema
        AddField(fieldDef);

        return this;
    }

    /// <summary>
    /// Extracts the member name from the lambda expression.
    /// </summary>
    private static string GetMemberNameFromExpression(LambdaExpression lambdaExpression)
    {
        if (lambdaExpression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        else if (lambdaExpression.Body is MemberExpression bodyMemberExpression)
        {
            return bodyMemberExpression.Member.Name;
        }

        throw new ArgumentException("Expression must be a member expression", nameof(lambdaExpression));
    }

    /// <summary>
    /// Gets the type of a member (property or field)
    /// </summary>
    private static Type GetMemberType(MemberInfo memberInfo)
    {
        if (memberInfo is PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType;
        }
        else if (memberInfo is FieldInfo fieldInfo)
        {
            return fieldInfo.FieldType;
        }

        return typeof(object);
    }
}
