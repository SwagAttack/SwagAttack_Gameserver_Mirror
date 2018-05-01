using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Models;
using Persistance.Interfaces;

namespace Persistance.Repository
{
    public class TestRepo : SwagRepository<IUser, User>
    {
        public TestRepo(IDbContext context, string collectionId) : base(context, collectionId)
        {
        }
    }
    
}