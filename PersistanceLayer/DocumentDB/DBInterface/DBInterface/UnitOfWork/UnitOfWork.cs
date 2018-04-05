using System;

using DBInterface.Repositories;

namespace DBInterface.UnitOfWork
{

    public class UnitOfWork : IunitOfWork
    {
        public readonly UserRepository UserRepository;
        private readonly DbContext _context;
        public UnitOfWork(DbContext context)
        {
            _context = context;
            UserRepository = new UserRepository(_context);

        }
    }
}
