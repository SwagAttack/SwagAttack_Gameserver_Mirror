using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Persistance.Interfaces;

namespace Persistance.Setup
{
    public class DbContext : IDbContext
    {
        private IDocumentClient _client;

        public string DatabaseId { get; set; } = "UserDB"; // XD

        // Local
        private static string AuthenticationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private static string ServiceEndpoint = "https://localhost:8081";

        // Remote

        //private static string AuthenticationKey = "https://swagattack.documents.azure.com:443/";
        //private static string ServiceEndpoint = "DDqKAMshqSd0cktDjmGqZSUFprEFgGD44Eo6FLOfK9CmuJVCLG7K7blhV2YL0yRpir5kTVuarKmuriXNKw0flg==";

        public IDocumentClient DocumentClient
        {
            get => _client;
            set => _client = value;
        }

        public DbContext(string databaseId = null, IDocumentClient  client = null)
        {
            if (databaseId != null) DatabaseId = databaseId;
            if (client == null)_client = new DocumentClient(new Uri(ServiceEndpoint), AuthenticationKey);
            CreateDatabaseIfNotExists();
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                if(_client.ReadDatabaseAsync(DatabaseId).IsFaulted)
                    _client.CreateDatabaseAsync(new Database() {Id = DatabaseId}).Wait();
            }
            catch (DocumentClientException)
            {

            }
            catch (Exception) // Error occured - throw
            {
                throw;
            }

        }
    }
}