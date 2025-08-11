using System.Linq.Expressions;
using System.Reflection;

namespace MegaMapper;

/// <summary>
/// Reflection for lambdas
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Extracts the <see cref="MemberInfo"/> from a lambda expression like <c>x => x.Property</c> or <c>x => x.Nested.Property</c>.
    /// Handles optional conversions (e.g., casts to object).
    /// </summary>
    /// <param name="lambdaExpression">A lambda expression that accesses a property or field.</param>
    /// <returns>The <see cref="MemberInfo"/> of the last accessed member.</returns>
    /// <exception cref="ArgumentException">Thrown if the expression does not refer to a member.</exception>
    public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
    {
        if (lambdaExpression == null)
            throw new ArgumentNullException(nameof(lambdaExpression));

        Expression body = lambdaExpression.Body;

        // Remove any conversion (e.g., (object)x.Prop)
        while (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
        {
            body = unary.Operand;
        }

        if (body is MemberExpression memberExpr)
        {
            return memberExpr.Member;
        }

        throw new ArgumentException(
            $"Expression '{lambdaExpression}' must refer to a field or property (e.g., x => x.Property or x => x.Nested.Property).",
            nameof(lambdaExpression));
    }


    /// <summary>
    /// Sets the value of a property or field represented by a <see cref="MemberInfo"/> on the specified target object.
    /// </summary>
    /// <param name="propertyOrField">The member to set (must be a <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>).</param>
    /// <param name="target">The object whose member value will be set.</param>
    /// <param name="value">The value to set.</param>
    /// <exception cref="ArgumentException">Thrown if the member is not a property or field, or if it's read-only.</exception>
    public static void SetMemberValue(this MemberInfo propertyOrField, object target, object? value)
    {
        if (propertyOrField is PropertyInfo prop)
        {
            if (!prop.CanWrite)
                throw new ArgumentException($"Property '{prop.Name}' is read-only.", nameof(propertyOrField));

            prop.SetValue(target, value);
        }
        else if (propertyOrField is FieldInfo field)
        {
            field.SetValue(target, value);
        }
        else
        {
            throw new ArgumentException(
                $"Member '{propertyOrField.Name}' is not a property or field.",
                nameof(propertyOrField));
        }
    }

    /// <summary>
    /// Gets the value of a property or field represented by a <see cref="MemberInfo"/> from the specified target object.
    /// </summary>
    /// <param name="propertyOrField">The member to get (must be a <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>).</param>
    /// <param name="target">The object whose member value will be retrieved.</param>
    /// <returns>The value of the member, or null if the member does not have a value.</returns>
    /// <exception cref="ArgumentException">Thrown if the member is not a property or field.</exception>
    public static object? GetMemberValue(this MemberInfo propertyOrField, object target)
    {
        if (propertyOrField is PropertyInfo prop)
        {
            return prop.GetValue(target);
        }
        else if (propertyOrField is FieldInfo field)
        {
            return field.GetValue(target);
        }
        else
        {
            throw new ArgumentException(
                $"Member '{propertyOrField.Name}' is not a property or field.",
                nameof(propertyOrField));
        }
    }
}