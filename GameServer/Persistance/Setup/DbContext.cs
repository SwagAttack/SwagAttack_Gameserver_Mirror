using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Persistance.Interfaces;

namespace Persistance.Setup
{
    public class DbContext : IDbContext
    {
        private DocumentClient _client;

        public string DatabaseId { get; set; } = "SwagDb"; // XD

        // Local
        private static string AuthenticationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private static string ServiceEndpoint = "https://localhost:8081";

        // Remote

        //private static string AuthenticationKey = "https://swagattack.documents.azure.com:443/";
        //private static string ServiceEndpoint = "DDqKAMshqSd0cktDjmGqZSUFprEFgGD44Eo6FLOfK9CmuJVCLG7K7blhV2YL0yRpir5kTVuarKmuriXNKw0flg==";

        public DocumentClient DocumentClient => _client;

        public DbContext(string databaseId = null)
        {
            if (databaseId != null) DatabaseId = databaseId;
            _client = new DocumentClient(new Uri(ServiceEndpoint), AuthenticationKey);
            CreateDatabaseIfNotExists();
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                _client.CreateDatabaseIfNotExistsAsync(new Database() {Id = DatabaseId}).Wait();
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