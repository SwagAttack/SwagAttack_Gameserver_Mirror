using System;
using System.Collections.Generic;
using System.Text;
using Persistance.Interfaces;

namespace Domain.Interfaces
{
    public interface IGameRepository : IRepository<IGamePlayerInfo>
    {
    }
}
