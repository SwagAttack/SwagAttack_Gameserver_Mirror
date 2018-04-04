using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using User = Models.User.User;

//https://github.com/Azure/azure-documentdb-dotnet/blob/f374cc601f4cf08d11c88f0c3fa7dcefaf7ecfe8/samples/code-samples/DocumentManagement/Program.cs#L198
namespace DBInterface.Repositories
{
    public class UserRepository : Repository<User>
    {

        public UserRepository(DbContext context) : base(context)
        {
        }

        public async Task AddUser(User thisUser)
        {
            try
            {
                GetUserByEmail(thisUser.Email);
                Console.WriteLine("user already exsist!");
            }
            catch (Exception e)
            {
                Document created = await _Context.UserClient.CreateDocumentAsync(_Context.UserCollection.DocumentsLink, thisUser);
                Console.WriteLine(created);

            }

        }

        public User GetUserByEmail(string id)
        {

            User TheUser = _Context.UserClient.CreateDocumentQuery<User>(_Context.UserCollection.DocumentsLink)
                .Where(x => x.Email == id)
                .AsEnumerable()
                .FirstOrDefault();
            if (TheUser == null)
            {
                throw new ArgumentException("User dosent exsist");
            }

            return TheUser;
        }

        public void DeleteUserByEmail(string Email)
        {
            Document doc = GetDoc(Email);
            if (doc != null)
            {
                _Context.UserClient.DeleteDocumentAsync(doc.SelfLink).Wait();
            }
        }

        public void ReplaceUser(User ThisUser)
        {

            Document doc = GetDoc(ThisUser.Email);
            _Context.UserClient.ReplaceDocumentAsync(doc.SelfLink, ThisUser).Wait();
        }

        private Document GetDoc(string id)
        {
            return _Context.UserClient.CreateDocumentQuery(_Context.UserCollection.DocumentsLink).Where(x => x.Id == id)
                .AsEnumerable().FirstOrDefault();
        }
    }
}
