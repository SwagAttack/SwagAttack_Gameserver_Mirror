using Microsoft.Azure.Documents.Client;

namespace Persistance.Interfaces
{
    public interface IDbContext
    {
        string DatabaseName { get; }
        DocumentClient DocumentClient { get; }
    }
}