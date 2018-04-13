using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

//https://github.com/Azure/azure-documentdb-dotnet/blob/f374cc601f4cf08d11c88f0c3fa7dcefaf7ecfe8/samples/code-samples/DocumentManagement/Program.cs#L198
namespace Persistance.Repositories
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

            IUser theUser = GetContext().UserClient.CreateDocumentQuery<Domain.Models.User>(GetContext().UserCollection.DocumentsLink)
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

        async Task<Domain.Interfaces.IUser> IUserRepository.GetUserByUsernameAsync(string id)
        {
            try
            {

                var document = await GetContext().UserClient.ReadDocumentAsync<Domain.Models.User>(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
                var user = (Domain.Models.User)(dynamic)document;
                return user as Domain.Interfaces.IUser;
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
