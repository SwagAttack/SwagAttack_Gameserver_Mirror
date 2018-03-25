using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Models.Interfaces;
using Models.User;
using User = Models.User.User;

//https://github.com/Azure/azure-documentdb-dotnet/blob/f374cc601f4cf08d11c88f0c3fa7dcefaf7ecfe8/samples/code-samples/DocumentManagement/Program.cs#L198
namespace DBInterface.DAL
{
    public class UserRepository<T> where T : class
    {
        private const string EndpointUrl = "https://localhost:8081";
        private const string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private DocumentClient Client;
        private DocumentCollection documentCollection;
        public UserRepository()
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
            finally
            {
                Console.WriteLine("End of program");
                Console.ReadKey();
            }

        }

        private async Task LoadDB()
        {
            Client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            await Client.CreateDatabaseIfNotExistsAsync(new Database { Id = "UserDB" });
            documentCollection =  await Client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("UserDB"),
                new DocumentCollection { Id = "UserCollection" });
        }

        public async Task AddUser(User thisUser)
        {
            Document created = await Client.CreateDocumentAsync(documentCollection.DocumentsLink, thisUser);
            Console.WriteLine(created);
        }

        public async Task<User> GetUserById(string id)
        {
            User querySalesOrder = Client.CreateDocumentQuery<User>(documentCollection.DocumentsLink)
                .Where(so => so.Email == id)
                .AsEnumerable()
                .FirstOrDefault();
            return querySalesOrder;
        }

    }
}
