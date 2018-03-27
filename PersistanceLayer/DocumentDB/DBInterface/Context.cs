using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace DBInterface
{
    public class DbContext
    {
        private const string EndpointUrl = "https://localhost:8081";
        private const string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        /// <summary>
        /// For all connection to database
        /// </summary>
        public DocumentClient UserClient;
        /// <summary>
        /// for information about the users
        /// </summary>
        public DocumentCollection UserCollection;

        /// <summary>
        /// Set up the database connection
        /// </summary>
        public DbContext()
        {
            try
            {
                LoadDB().Wait();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Husk at starte emulator op!");
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }
        /// <summary>
        /// The method to both get/set database and collection(s)
        /// </summary>
        /// <returns></returns>
        private async Task LoadDB()
        {
            UserClient = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            await UserClient.CreateDatabaseIfNotExistsAsync(new Database { Id = "UserDB" });
            UserCollection = await UserClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("UserDB"),
                new DocumentCollection { Id = "UserCollection" });
        }
    }
}
