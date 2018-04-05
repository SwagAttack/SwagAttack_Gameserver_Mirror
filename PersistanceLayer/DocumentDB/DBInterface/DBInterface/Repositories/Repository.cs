namespace DBInterface.Repositories
{
    public class Repository<T> where T : class
    {
        protected readonly DbContext Context;

        public Repository(DbContext context)
        {
            Context = context;
        }

    }
}
