using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Persistance.Interfaces
{
    public interface IRepository<TInterface, TEntity> where TEntity : TInterface
                                                      where TInterface : class
    {
        Task<TInterface> GetItemAsync(string id);
        Task<IEnumerable<TInterface>> GetItemsAsync(Expression<Func<TInterface, bool>> predicate);
        Task<TInterface> UpdateItemAsync(string id, TInterface item);
        Task<TInterface> CreateItemAsync(TInterface item);
        Task<TInterface> DeleteItemAsync(string id, TInterface item);
    }
}