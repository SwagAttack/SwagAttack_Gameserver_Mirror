using System;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Persistance.Interfaces;

namespace Persistance.Setup
{
    public class DbContext : IDbContext
    {
        public string DatabaseId { get; set; } = "UserDB"; // XD

        // Local
        private static string AuthenticationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private static string ServiceEndpoint = "https://localhost:8081";

        // Remote

        //private static string AuthenticationKey = "https://swagattack.documents.azure.com:443/";
        //private static string ServiceEndpoint = "DDqKAMshqSd0cktDjmGqZSUFprEFgGD44Eo6FLOfK9CmuJVCLG7K7blhV2YL0yRpir5kTVuarKmuriXNKw0flg==";

        public IDocumentClient DocumentClient { get; set; }

        public DbContext(string databaseId = null, IDocumentClient  client = null)
        {
            if (databaseId != null) DatabaseId = databaseId;
            DocumentClient = client ?? new DocumentClient(new Uri(ServiceEndpoint), AuthenticationKey);
            CreateDatabaseIfNotExists();
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                DocumentClient.CreateDatabaseAsync(new Database() {Id = DatabaseId}).Wait();
            }
            catch (AggregateException e)
            {
                e.Handle(ex =>
                {
                    if (ex is DocumentClientException de)
                    {
                        if (de.StatusCode == HttpStatusCode.Conflict)
                            return true;
                    }
                    return false;
                });
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                    return;
                throw;
            }           
        }
    }
}