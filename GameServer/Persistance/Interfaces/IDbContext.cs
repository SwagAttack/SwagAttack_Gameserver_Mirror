using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Persistance.Interfaces
{
    public interface IDbContext
    {
        string DatabaseId { get; }
        IDocumentClient DocumentClient { get; }
    }
}