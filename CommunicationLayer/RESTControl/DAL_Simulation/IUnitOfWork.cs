using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models.Interfaces;

namespace RESTControl.DAL_Simulation
{
    /// <summary>
    /// Mock UnitOfWork for Users. Used for testing purposes
    /// </summary>
    public interface IUnitOfWork
    {
        ICollection<IUser> Users { get; }
    }
}
