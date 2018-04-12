using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Models.Interfaces;

//https://github.com/Azure/azure-documentdb-dotnet/blob/f374cc601f4cf08d11c88f0c3fa7dcefaf7ecfe8/samples/code-samples/DocumentManagement/Program.cs#L198
namespace DBInterface.Repositories
{
    public class UserRepository : Repository<IUser>, IUserRepository
    {
        private static readonly string DatabaseId = "UserDB";
        private static readonly string CollectionId = "UserCollection";

        public UserRepository(DbContext context) : base(context)
        {
        }

        public void AddUser(IUser thisUser)
        {
            if (GetUserByUsername(thisUser.Username) == null)
            {
                GetContext().UserClient.CreateDocumentAsync(GetContext().UserCollection.DocumentsLink, thisUser).Wait();
            }
        }

        public IUser GetUserByUsername(string id)
        {

            IUser theUser = GetContext().UserClient.CreateDocumentQuery<Models.User.User>(GetContext().UserCollection.DocumentsLink)
                .Where(x => x.Username == id)
                .AsEnumerable()
                .FirstOrDefault();

            return theUser;
        }

        public void DeleteUserByUsername(string username)
        {
            Document doc = GetDoc(username);
            if (doc != null)
            {
                GetContext().UserClient.DeleteDocumentAsync(doc.SelfLink).Wait();
            }
        }

        public void ReplaceUser(IUser thisUser)
        {
            Document doc = GetDoc(thisUser.Username);
            GetContext().UserClient.ReplaceDocumentAsync(doc.SelfLink, thisUser).Wait();
        }

        private Document GetDoc(string id)
        {
            return GetContext().UserClient.CreateDocumentQuery(GetContext().UserCollection.DocumentsLink).Where(x => x.Id == id)
                .AsEnumerable().FirstOrDefault();
        }

        async Task<Document> IUserRepository.GetUserByUsernameAsync(string id)
        {
            try
            {
                Document document = await GetContext().UserClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
                return document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

    }
}
