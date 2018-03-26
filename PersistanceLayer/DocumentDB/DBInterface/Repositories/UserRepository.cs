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
                
                Document created = await _Context.Client.CreateDocumentAsync(_Context.UserCollection.DocumentsLink, thisUser);
                Console.WriteLine(created);
            }

        }

        public User GetUserByEmail(string id)
        {

            User TheUser = _Context.Client.CreateDocumentQuery<User>(_Context.UserCollection.DocumentsLink)
                .Where(so => so.Email == id)
                .AsEnumerable()
                .FirstOrDefault();
            if (TheUser == null)
            {
                throw new ArgumentException("User dosent exsist");
            }
            
            return TheUser;
        }

    }
}
