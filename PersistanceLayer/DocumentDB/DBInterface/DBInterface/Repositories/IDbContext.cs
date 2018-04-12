using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace DBInterface.Repositories
{
    public interface IDbContext
    {
        /// <summary>
        /// For all connection to database
        /// </summary>
        DocumentClient UserClient { get; set; }
        /// <summary>
        /// for information about the users
        /// </summary>
        DocumentCollection UserCollection { get; set; }
    }
}