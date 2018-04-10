namespace Persistance.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public readonly DbContext Context;
        private DbContext _context;

        public Repository(DbContext context)
        {
            Context = context;
        }
    }
}
