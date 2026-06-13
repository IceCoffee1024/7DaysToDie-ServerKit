using IceCoffee.SimpleCRUD;

namespace SdtdServerKit.Data
{
    public abstract class DefaultRepository<T> : RepositoryBase<T>
    {
        public DefaultRepository() : base(DbConnectionFactory.Default, DbAliases.Default)
        {
        }
    }
}