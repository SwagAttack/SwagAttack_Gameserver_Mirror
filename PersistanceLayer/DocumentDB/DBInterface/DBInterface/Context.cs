using System;
using System.Threading.Tasks;
using DBInterface.Repositories;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace DBInterface
{
    public class DbContext : IDbContext
    {
        //local

        //private const string EndpointUrl = "https://localhost:8081";
        //private const string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        //Out

        private const string EndpointUrl = "https://swagattackuserdb.documents.azure.com:443/";
        private const string PrimaryKey = "YAKAaZw4Rjx8eVV3WSvWbIU9P4yjgBxbBmDYSR51dCWc7bqlkW1088MPvEZRsdB5KHPL30NiN8hnRnFk0I3B6w==";

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
