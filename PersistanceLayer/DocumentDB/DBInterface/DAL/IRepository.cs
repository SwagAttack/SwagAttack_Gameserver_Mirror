using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace DBInterface.DAL
{
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Returns an Enumerable of the items
        /// </summary>
        /// <param name="predicate">the specifications for the items</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Creates a new item in the database
        /// </summary>
        /// <param name="item">The instance of the item</param>
        /// <returns></returns>
        Task<Document> CreateItemAsync(T item);
        /// <summary>
        /// Replace the item 
        /// </summary>
        /// <param name="id"> id of item</param>
        /// <param name="item">replacing item</param>
        /// <returns></returns>
        Task<Document> UpdateItemAsync(string id, T item);
        /// <summary>
        /// Deleting an item from the database
        /// </summary>
        /// <param name="id">id of the item to delete</param>
        /// <returns></returns>
        Task DeleteItemAsync(string id);
        /// <summary>
        /// Returns a desired item from the database
        /// </summary>
        /// <param name="id">the id of of the item</param>
        /// <returns></returns>
        Task<T> GetItemAsync(string id);
        /// <summary>
        /// setup the db and documents
        /// </summary>
        void Initialize(); 
        //IEnumerable<T> GetAll();
        //IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        //T Single(Expression<Func<T, bool>> predicate);
        //T SingleOrDefault(Expression<Func<T, bool>> predicate);
        //T First(Expression<Func<T, bool>> predicate);
        //T GetById(int id);

        //void Add(T entity);
        //void Delete(T entity);
        //void Attach(T entity);
    }
}
