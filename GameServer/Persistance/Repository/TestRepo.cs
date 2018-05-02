using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Persistance.Interfaces;
using User = Domain.Models.User;

namespace Persistance.Repository
{
    public class TestRepo : SwagRepository<IUser, User>
    {
        public TestRepo(IDbContext context, string collectionId) : base(context, collectionId)
        {
        }

    }



}