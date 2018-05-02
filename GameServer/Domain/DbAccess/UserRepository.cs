using Domain.Interfaces;
using Domain.Models;
using Persistance.Interfaces;
using Persistance.Repository;

namespace Domain.DbAccess
{
    public class UserRepository : SwagRepository<IUser, User>, IUserRepository
    {
        public UserRepository(IDbContext context, string collectionId) : base(context, collectionId)
        {
        }
    }
}