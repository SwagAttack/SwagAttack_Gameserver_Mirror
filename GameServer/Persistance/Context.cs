using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Persistance.Interfaces;

namespace Persistance
{
    public class DbContext : IDbContext
    {
        //local

        //private const string EndpointUrl = "https://localhost:8081";
        //private const string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        //Out

        private const string EndpointUrl = "https://swagattack.documents.azure.com:443/";
        private const string PrimaryKey = "DDqKAMshqSd0cktDjmGqZSUFprEFgGD44Eo6FLOfK9CmuJVCLG7K7blhV2YL0yRpir5kTVuarKmuriXNKw0flg==";

        public DocumentClient UserClient { get; set; }
        public DocumentCollection UserCollection { get; set; }

        /// <summary>
        /// Set up the database connection
        /// </summary>
        public DbContext()
        {
            try
            {
                LoadDb().Wait();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();

            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
            }

        }
        /// <summary>
        /// The method to both get/set database and collection(s)
        /// </summary>
        /// <returns></returns>
        private async Task LoadDb()
        {
            UserClient = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            await UserClient.CreateDatabaseIfNotExistsAsync(new Database { Id = "UserDB" });

            UserCollection = await UserClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("UserDB"),
            new DocumentCollection { Id = "UserCollection" });
        }

    }
}
