using System;
using DBInterface.DAL;
using Models;

namespace DBInterface.UnitOfWork
{
    public interface IUnitOfWork<T> : IDisposable
    {

        void Commit();
    }
}
