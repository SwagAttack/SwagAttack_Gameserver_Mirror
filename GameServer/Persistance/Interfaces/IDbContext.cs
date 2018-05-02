using Microsoft.Azure.Documents.Client;

namespace Persistance.Interfaces
{
    public interface IDbContext
    {
        string DatabaseId { get; }
        DocumentClient DocumentClient { get; }
    }
}