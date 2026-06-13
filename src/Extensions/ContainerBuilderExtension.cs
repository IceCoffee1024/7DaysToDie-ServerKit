using IceCoffee.SimpleCRUD;
using System.Reflection;

namespace SdtdServerKit.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="ContainerBuilder"/> class.
    /// </summary>
    internal static class ContainerBuilderExtension
    {
        /// <summary>
        /// Adds repositories to the container builder.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="assembly">The assembly containing the repositories.</param>
        /// <returns>The container builder.</returns>
        public static ContainerBuilder AddRepositories(this ContainerBuilder builder, Assembly assembly)
        {
            builder.RegisterInstance(DbConnectionFactory.Default).As<IDbConnectionFactory>();
            builder.RegisterType<SdtdServerKit.Data.RepositoryFactory>().As<IRepositoryFactory>().SingleInstance();
            builder.RegisterType<UnitOfWorkFactory>().As<IUnitOfWorkFactory>().SingleInstance();

            foreach (var implType in assembly.GetExportedTypes())
            {
                if (implType.IsSubclassOf(typeof(RepositoryBase)) && implType.IsAbstract == false && implType.IsGenericType == false)
                {
                    var serviceType = implType.GetInterfaces().First(p => typeof(IRepository).IsAssignableFrom(p) && p != typeof(IRepository) && p.IsGenericType == false);
                    builder.RegisterType(implType).As(serviceType).SingleInstance();
                }
            }

            return builder;
        }
    }
}
