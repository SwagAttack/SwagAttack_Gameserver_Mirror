using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Persistance.Interfaces;

namespace Persistance.Repository
{

    public class SwagRepository<TInterface, TEntity> : IRepository<TInterface, TEntity> where TEntity : TInterface
                                                                               where TInterface : class
    {
        protected IDbContext Context;
        protected string CollectionId;

        protected SwagRepository(IDbContext context, string collectionId)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            CollectionId = collectionId ?? throw new ArgumentNullException(nameof(collectionId));
            CreateCollectionIfNotExists();
        }
        public virtual async Task<IEnumerable<TInterface>> GetItemsAsync(Expression<Func<TInterface, bool>> predicate)
        {
            var pred = ExpressionTreeModifier.TransformPredicateLambda<TInterface, TEntity>(predicate);
            var query = Context.DocumentClient.CreateDocumentQuery<TEntity>(
                    UriFactory.CreateDocumentCollectionUri(Context.DatabaseName, CollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(pred)
                .AsDocumentQuery();

            var results = new List<TEntity>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<TEntity>());
            }

            return results as IEnumerable<TInterface>;
        }

        public async Task<TInterface> UpdateItemAsync(string id, TInterface item)
        {
            try
            {
                var result = await Context.DocumentClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(Context.DatabaseName, CollectionId, id), item);
                return (TEntity) (dynamic) result.Resource;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        public async Task<TInterface> CreateItemAsync(TInterface item)
        {
            try
            {
                var result = await Context.DocumentClient.CreateDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri(Context.DatabaseName, CollectionId), item);
                return (TEntity)(dynamic)result.Resource;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict) // Already exists
                {
                    return null;
                }

                throw;
            }
        }

        public async Task<TInterface> DeleteItemAsync(string id, TInterface item)
        {
            try
            {
                var result = await Context.DocumentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Context.DatabaseName, CollectionId, id));
                return (TEntity)(dynamic)result.Resource;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        private void CreateCollectionIfNotExists()
        {
            try
            {
                Context.DocumentClient.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(Context.DatabaseName),
                    new DocumentCollection() {Id = CollectionId}).Wait();
            }
            catch (DocumentClientException)
            {

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TInterface> GetItemAsync(string id)
        {
            try
            {
                Document document = await Context.DocumentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(Context.DatabaseName, CollectionId, id));
                return (TEntity)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

    }

    /// <summary>
    /// Inspired by:
    /// https://stackoverflow.com/questions/2797261/mutating-the-expression-tree-of-a-predicate-to-target-another-type/10467736#10467736
    /// </summary>
    internal class ExpressionTreeModifier
    {
        public static Expression<Func<TNewTarget, bool>>
        TransformPredicateLambda<TOldTarget, TNewTarget>(
        Expression<Func<TOldTarget, bool>> predicate)
        {
            var lambda = (LambdaExpression)predicate;
            if (lambda == null)
            {
                throw new NotSupportedException();
            }

            var mutator = new ExpressionTargetTypeMutator(t => typeof(TNewTarget));
            var explorer = new ExpressionTreeExplorer();
            var converted = mutator.Visit(predicate.Body);

            return Expression.Lambda<Func<TNewTarget, bool>>(
                converted,
                lambda.Name,
                lambda.TailCall,
                explorer.Explore(converted).OfType<ParameterExpression>());
        }


        private class ExpressionTargetTypeMutator : ExpressionVisitor
        {
            private readonly Func<Type, Type> typeConverter;

            public ExpressionTargetTypeMutator(Func<Type, Type> typeConverter)
            {
                this.typeConverter = typeConverter;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var dataContractType = node.Member.ReflectedType;
                var activeRecordType = this.typeConverter(dataContractType);

                var converted = Expression.MakeMemberAccess(
                    base.Visit(node.Expression),
                    activeRecordType.GetProperty(node.Member.Name));

                return converted;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                var dataContractType = node.Type;
                var activeRecordType = this.typeConverter(dataContractType);

                return Expression.Parameter(activeRecordType, node.Name);
            }
        }
    }

    /// <summary>
    /// Utility class for the traversal of expression trees.
    /// </summary>
    public class ExpressionTreeExplorer
    {
        private readonly Visitor visitor = new Visitor();

        /// <summary>
        /// Returns the enumerable collection of expressions that comprise
        /// the expression tree rooted at the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// The enumerable collection of expressions that comprise the expression tree.
        /// </returns>
        public IEnumerable<Expression> Explore(Expression node)
        {
            return this.visitor.Explore(node);
        }

        private class Visitor : ExpressionVisitor
        {
            private readonly List<Expression> expressions = new List<Expression>();

            protected override Expression VisitBinary(BinaryExpression node)
            {
                this.expressions.Add(node);
                return base.VisitBinary(node);
            }

            protected override Expression VisitBlock(BlockExpression node)
            {
                this.expressions.Add(node);
                return base.VisitBlock(node);
            }

            protected override Expression VisitConditional(ConditionalExpression node)
            {
                this.expressions.Add(node);
                return base.VisitConditional(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                this.expressions.Add(node);
                return base.VisitConstant(node);
            }

            protected override Expression VisitDebugInfo(DebugInfoExpression node)
            {
                this.expressions.Add(node);
                return base.VisitDebugInfo(node);
            }

            protected override Expression VisitDefault(DefaultExpression node)
            {
                this.expressions.Add(node);
                return base.VisitDefault(node);
            }

            protected override Expression VisitDynamic(DynamicExpression node)
            {
                this.expressions.Add(node);
                return base.VisitDynamic(node);
            }

            protected override Expression VisitExtension(Expression node)
            {
                this.expressions.Add(node);
                return base.VisitExtension(node);
            }

            protected override Expression VisitGoto(GotoExpression node)
            {
                this.expressions.Add(node);
                return base.VisitGoto(node);
            }

            protected override Expression VisitIndex(IndexExpression node)
            {
                this.expressions.Add(node);
                return base.VisitIndex(node);
            }

            protected override Expression VisitInvocation(InvocationExpression node)
            {
                this.expressions.Add(node);
                return base.VisitInvocation(node);
            }

            protected override Expression VisitLabel(LabelExpression node)
            {
                this.expressions.Add(node);
                return base.VisitLabel(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                this.expressions.Add(node);
                return base.VisitLambda(node);
            }

            protected override Expression VisitListInit(ListInitExpression node)
            {
                this.expressions.Add(node);
                return base.VisitListInit(node);
            }

            protected override Expression VisitLoop(LoopExpression node)
            {
                this.expressions.Add(node);
                return base.VisitLoop(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                this.expressions.Add(node);
                return base.VisitMember(node);
            }

            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                this.expressions.Add(node);
                return base.VisitMemberInit(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                this.expressions.Add(node);
                return base.VisitMethodCall(node);
            }

            protected override Expression VisitNew(NewExpression node)
            {
                this.expressions.Add(node);
                return base.VisitNew(node);
            }

            protected override Expression VisitNewArray(NewArrayExpression node)
            {
                this.expressions.Add(node);
                return base.VisitNewArray(node);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                this.expressions.Add(node);
                return base.VisitParameter(node);
            }

            protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
            {
                this.expressions.Add(node);
                return base.VisitRuntimeVariables(node);
            }

            protected override Expression VisitSwitch(SwitchExpression node)
            {
                this.expressions.Add(node);
                return base.VisitSwitch(node);
            }

            protected override Expression VisitTry(TryExpression node)
            {
                this.expressions.Add(node);
                return base.VisitTry(node);
            }

            protected override Expression VisitTypeBinary(TypeBinaryExpression node)
            {
                this.expressions.Add(node);
                return base.VisitTypeBinary(node);
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                this.expressions.Add(node);
                return base.VisitUnary(node);
            }

            public IEnumerable<Expression> Explore(Expression node)
            {
                this.expressions.Clear();
                this.Visit(node);
                return expressions.ToArray();
            }
        }
    }

}