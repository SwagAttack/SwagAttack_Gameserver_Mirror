using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models.Interfaces;

namespace RESTControl.DAL_Simulation
{
    public interface IUnitOfWork
    {
        ICollection<IUser> Users { get; }
    }
}
