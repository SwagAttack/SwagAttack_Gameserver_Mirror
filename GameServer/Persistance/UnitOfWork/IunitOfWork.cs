using Persistance.Repositories;

namespace Persistance.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
    }
}
