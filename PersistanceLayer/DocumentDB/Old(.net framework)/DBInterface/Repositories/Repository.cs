namespace DBInterface.Repositories
{
    public class Repository<T> where T:class
    {
        protected readonly DbContext _Context;

        public Repository(DbContext context)
        {
            _Context = context;
        }

    }
}
