using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Models.Interfaces;

//https://github.com/Azure/azure-documentdb-dotnet/blob/f374cc601f4cf08d11c88f0c3fa7dcefaf7ecfe8/samples/code-samples/DocumentManagement/Program.cs#L198
namespace DBInterface.Repositories
{
    public class UserRepository : Repository<IUser>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {
        }

        public void AddUserAsyncTask(IUser thisUser)
        {
            if (GetUserByUsername(thisUser.Username) == null)
            {
                Context.UserClient.CreateDocumentAsync(Context.UserCollection.DocumentsLink, thisUser).Wait();
            }
        }

        public IUser GetUserByUsername(string id)
        {

            IUser theUser = Context.UserClient.CreateDocumentQuery<Models.User.User>(Context.UserCollection.DocumentsLink)
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
                Context.UserClient.DeleteDocumentAsync(doc.SelfLink).Wait();
            }
        }

        public void ReplaceUser(IUser thisUser)
        {
            Document doc = GetDoc(thisUser.Username);
            Context.UserClient.ReplaceDocumentAsync(doc.SelfLink, thisUser).Wait();
        }

        private Document GetDoc(string id)
        {
            return Context.UserClient.CreateDocumentQuery(Context.UserCollection.DocumentsLink).Where(x => x.Id == id)
                .AsEnumerable().FirstOrDefault();
        }
    }
}
