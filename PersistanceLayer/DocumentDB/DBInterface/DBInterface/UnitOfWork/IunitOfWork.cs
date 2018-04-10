using System;
using System.Collections.Generic;
using System.Text;
using DBInterface.Repositories;

namespace DBInterface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
    }
}
