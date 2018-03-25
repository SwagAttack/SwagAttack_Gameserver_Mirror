using System.Collections.Generic;
using Models.Interfaces;

namespace RESTControl.DAL_Simulation
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICollection<IUser> Users => _users ?? (_users = new List<IUser>());
        private List<IUser> _users;
    }
}