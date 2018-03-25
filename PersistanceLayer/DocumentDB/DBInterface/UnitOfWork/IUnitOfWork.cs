using System;
using DBInterface.DAL;
using Models;

namespace DBInterface.UnitOfWork
{
    public interface IUnitOfWork<T> : IDisposable
    {
        //identify needed unit of work
        IGenericRepository<Bruger> BrugeRepository { get; }

        void Commit();
    }
}
