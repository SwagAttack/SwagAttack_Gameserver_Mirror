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
using Newtonsoft.Json;
using Persistance.Interfaces;
using Persistance.Misc;

namespace Persistance.Repository
{

    public class SwagRepository<TInterface, TEntity> : IRepository<TInterface> where TEntity : TInterface
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
            var pred = PredicateConverter.TransformPredicateLambda<TInterface, TEntity>(predicate);
            if(pred == null) throw new InvalidCastException($"Cannot convert from {typeof(TInterface)} to {typeof(TEntity)}!");

            var query = Context.DocumentClient.CreateDocumentQuery<TEntity>(
                    UriFactory.CreateDocumentCollectionUri(Context.DatabaseId, CollectionId),
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
                var result = await Context.DocumentClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(Context.DatabaseId, CollectionId, id), item);
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
                    UriFactory.CreateDocumentCollectionUri(Context.DatabaseId, CollectionId), item);
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

        public async Task<TInterface> DeleteItemAsync(string id)
        {
            try
            {
                var result = await Context.DocumentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Context.DatabaseId, CollectionId, id));
                return (TEntity)(dynamic)result.Resource;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        public async Task<TInterface> GetItemAsync(string id)
        {
            try
            {
                var result = await Context.DocumentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(Context.DatabaseId, CollectionId, id));
                return (TEntity) (dynamic) result.Resource;
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

        #region Creation

        private void CreateCollectionIfNotExists()
        {
            try
            {
                Context.DocumentClient.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(Context.DatabaseId),
                    new DocumentCollection() { Id = CollectionId }).Wait();

            }
            catch (AggregateException)
            {
                
            }
            catch (DocumentClientException)
            {

            }
        }

        #endregion

    }
}