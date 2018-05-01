using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
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
        public async Task<IEnumerable<TInterface>> GetItemsAsync(Expression<Func<TInterface, bool>> predicate)
        {
            var query = Context.DocumentClient.CreateDocumentQuery<TInterface>(
                    UriFactory.CreateDocumentCollectionUri(Context.DatabaseName, CollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
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
                return (TEntity)(dynamic)result;
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
                return (TEntity)(dynamic)result;
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
                return (TEntity)(dynamic)result;
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
}