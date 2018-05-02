using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Persistance.Misc
{
    /// <summary>
    /// Inspired by:
    /// https://stackoverflow.com/questions/2797261/mutating-the-expression-tree-of-a-predicate-to-target-another-type/10467736#10467736
    /// </summary>
    internal static class PredicateConverter
    {
        public static Expression<Func<TNewTarget, bool>>
        TransformPredicateLambda<TOldTarget, TNewTarget>(
        Expression<Func<TOldTarget, bool>> predicate)
        {
            var lambda = (LambdaExpression)predicate;

            if (lambda == null)
            {
                return null;
            }

            var mutator = new ExpressionTargetTypeMutator(t => typeof(TNewTarget));
            var explorer = new ExpressionTreeExplorer(); // For getting the parameter expressions
            var converted = mutator.Visit(lambda.Body);

            return Expression.Lambda<Func<TNewTarget, bool>>(
                converted,
                lambda.Name,
                lambda.TailCall,
                explorer.Explore(converted));
        }

        private class ExpressionTargetTypeMutator : ExpressionVisitor
        {
            private readonly Func<Type, Type> _typeConverter;

            public ExpressionTargetTypeMutator(Func<Type, Type> typeConverter)
            {
                _typeConverter = typeConverter;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var dataContractType = node.Member.ReflectedType;
                var activeRecordType = _typeConverter(dataContractType);

                var converted = Expression.MakeMemberAccess(
                    Visit(node.Expression),
                    activeRecordType.GetProperty(node.Member.Name));

                return converted;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                var dataContractType = node.Type;
                var activeRecordType = _typeConverter(dataContractType);

                return Expression.Parameter(activeRecordType, node.Name);
            }
        }
    }

    /// <summary>
    /// Utility class for the traversal of expression trees.
    /// </summary>
    public class ExpressionTreeExplorer
    {
        private readonly Visitor _visitor = new Visitor();

        /// <summary>
        /// Returns the enumerable collection of parameter-expressions that comprise
        /// the expression tree rooted at the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// The enumerable collection of expressions that comprise the expression tree.
        /// </returns>
        public IEnumerable<ParameterExpression> Explore(Expression node)
        {
            return _visitor.Explore(node);
        }

        private class Visitor : ExpressionVisitor
        {
            private readonly List<ParameterExpression> _expressions = new List<ParameterExpression>();

            protected override Expression VisitParameter(ParameterExpression node)
            {
                this._expressions.Add(node);
                return base.VisitParameter(node);
            }

            public IEnumerable<ParameterExpression> Explore(Expression node)
            {
                this._expressions.Clear();
                Visit(node);
                return _expressions.ToArray();
            }
        }
    }
}