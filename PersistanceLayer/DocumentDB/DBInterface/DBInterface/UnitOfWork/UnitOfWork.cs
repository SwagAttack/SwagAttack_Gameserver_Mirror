using System;

using DBInterface.Repositories;

namespace DBInterface.UnitOfWork
{

    public class UnitOfWork : IUnitOfWork
    {
        public readonly UserRepository _UserRepository;
        private readonly DbContext _context;
        public UnitOfWork(DbContext context)
        {
            _context = context;
            _UserRepository = new UserRepository(_context);

        }

        public UserRepository UserRepository => _UserRepository;
    }
}
