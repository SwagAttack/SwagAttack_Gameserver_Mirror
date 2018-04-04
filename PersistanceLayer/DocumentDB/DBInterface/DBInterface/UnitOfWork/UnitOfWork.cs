using System;

using DBInterface.Repositories;

namespace DBInterface.UnitOfWork
{

    public class UnitOfWork
    {
        public UserRepository _userRepository;
        private readonly DbContext _context;
        public UnitOfWork(DbContext context)
        {
            _context = context;
            _userRepository = new UserRepository(_context);

        }
    }
}
