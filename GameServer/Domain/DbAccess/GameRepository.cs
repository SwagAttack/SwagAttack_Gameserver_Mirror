using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Models;
using Persistance.Interfaces;
using Persistance.Repository;

namespace Domain.DbAccess
{
    public class GameRepository : SwagRepository<IGame, Game>, IGameRepository
    {
        public GameRepository(IDbContext context, string collectionId) : base(context, collectionId)
        {
        }
    }
}