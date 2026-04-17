using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace VK.Blocks.Core.Caches.Internal;

/// <summary>
/// A comparer that evaluates the equality of two expression trees.
/// </summary>
/// <remarks>
/// This is a simplified version of the ExpressionEqualityComparer, optimized for 
/// common repository usage such as property selectors and simple predicates.
/// </remarks>
internal sealed class ExpressionEqualityComparer : IEqualityComparer<Expression?>
{
    /// <summary>
    /// Gets the singleton instance of the comparer.
    /// </summary>
    public static ExpressionEqualityComparer Instance { get; } = new();

    private ExpressionEqualityComparer()
    {
    }

    /// <inheritdoc />
    public bool Equals(Expression? x, Expression? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (x.NodeType != y.NodeType || x.Type != y.Type)
        {
            return false;
        }

        return x switch
        {
            LambdaExpression lambdaX => y is LambdaExpression lambdaY &&
                                       lambdaX.Parameters.Count == lambdaY.Parameters.Count &&
                                       Equals(lambdaX.Body, lambdaY.Body),

            MemberExpression memberX => y is MemberExpression memberY &&
                                       memberX.Member == memberY.Member &&
                                       Equals(memberX.Expression, memberY.Expression),

            BinaryExpression binaryX => y is BinaryExpression binaryY &&
                                       binaryX.Method == binaryY.Method &&
                                       Equals(binaryX.Left, binaryY.Left) &&
                                       Equals(binaryX.Right, binaryY.Right),

            UnaryExpression unaryX => y is UnaryExpression unaryY &&
                                     unaryX.Method == unaryY.Method &&
                                     Equals(unaryX.Operand, unaryY.Operand),

            ParameterExpression parameterX => y is ParameterExpression parameterY &&
                                             parameterX.Name == parameterY.Name &&
                                             parameterX.Type == parameterY.Type,

            ConstantExpression constantX => y is ConstantExpression constantY &&
                                           Equals(constantX.Value, constantY.Value),

            MethodCallExpression methodX => y is MethodCallExpression methodY &&
                                           methodX.Method == methodY.Method &&
                                           Equals(methodX.Object, methodY.Object) &&
                                           CompareLists(methodX.Arguments, methodY.Arguments),

            NewExpression newX => y is NewExpression newY &&
                                 newX.Constructor == newY.Constructor &&
                                 CompareLists(newX.Arguments, newY.Arguments),

            _ => x.ToString() == y.ToString()
        };
    }

    /// <inheritdoc />
    public int GetHashCode([DisallowNull] Expression obj)
    {
        var hash = new HashCode();
        hash.Add(obj.NodeType);
        hash.Add(obj.Type);

        if (obj is MemberExpression member)
        {
            hash.Add(member.Member);
        }
        else if (obj is ConstantExpression constant)
        {
            hash.Add(constant.Value);
        }
        else if (obj is MethodCallExpression method)
        {
            hash.Add(method.Method);
        }

        return hash.ToHashCode();
    }

    private bool CompareLists<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
        where T : Expression
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Count; i++)
        {
            if (!Equals(left[i], right[i]))
            {
                return false;
            }
        }

        return true;
    }
}
