using Persistance.Repositories;
using Persistance.Repositories.old;

namespace Persistance.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
    }
}
