namespace DBInterface.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private static DbContext _context;

        public static DbContext GetContext()
        {
            return _context;
        }

        private static void SetContext(DbContext value)
        {
            _context = value;
        }

        public Repository(DbContext context)
        {
            SetContext(context);
        }

    }
}
