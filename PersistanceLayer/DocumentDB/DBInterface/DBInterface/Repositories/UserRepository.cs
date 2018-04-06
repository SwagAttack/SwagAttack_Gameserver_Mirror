using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

//https://github.com/Azure/azure-documentdb-dotnet/blob/f374cc601f4cf08d11c88f0c3fa7dcefaf7ecfe8/samples/code-samples/DocumentManagement/Program.cs#L198
namespace DBInterface.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(DbContext context) : base(context)
        {
        }

        public async Task AddUser(Models.User.User thisUser)
        {
            try
            {
                GetUserByUsername(thisUser.Username);
                Console.WriteLine("user already exsist!");
            }
            catch (Exception e)
            {
                Document created = await Context.UserClient.CreateDocumentAsync(Context.UserCollection.DocumentsLink, thisUser);
                Console.WriteLine(created);

            }

        }

        public Models.User.User GetUserByUsername(string id)
        {

            Models.User.User theUser = Context.UserClient.CreateDocumentQuery<Models.User.User>(Context.UserCollection.DocumentsLink)
                .Where(x => x.Username == id)
                .AsEnumerable()
                .FirstOrDefault();
            if (theUser == null)
            {
                throw new ArgumentException("User dosent exsist");
            }

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

        public void ReplaceUser(Models.User.User thisUser)
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
