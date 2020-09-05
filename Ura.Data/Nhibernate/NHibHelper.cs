using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using System.Linq;
using System.Reflection;
using Ura.Models;

namespace Ura.Data
{
    public class NHibernateHelper
    {
        private static Configuration _configuration;
        private static HbmMapping _mapping;
        private static ISessionFactory _sessionFactory;
        private static ISession _persistentSession = null;

        public static ISession OpenSession()
        {
            var session = SessionFactory.OpenSession();
            session.FlushMode = FlushMode.Commit;
            return session;
        }

        public static ISession GetPersistentSession()
        {
            if (_persistentSession == null)
            {
                _persistentSession = SessionFactory.OpenSession();
            }

            return _persistentSession;
        }

        public static Configuration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = CreateConfiguration();
                }
                return _configuration;
            }
        }

        private static HbmMapping Mapping
        {
            get
            {
                return _mapping ?? (_mapping = CreateMapping());
            }
        }

        private static ISessionFactory SessionFactory
        {
            get
            {
                return _sessionFactory ?? (_sessionFactory = Configuration.BuildSessionFactory());
            }
        }

        private static Configuration CreateConfiguration()
        {
            var cfg = new Configuration();

            cfg.Configure("NHibernate\\hibernate.cfg.xml");
            cfg.Properties[Environment.CollectionTypeFactoryClass] = typeof(Net4CollectionTypeFactory).AssemblyQualifiedName;
            cfg.AddAssembly(typeof(User).Assembly);
            cfg.AddMapping(Mapping);
            cfg.AddXmlFile("NHibMappings/User.hbm.xml");

            return cfg;
        }

        private static HbmMapping CreateMapping()
        {
            var mapper = new ModelMapper();
            var types = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => t.Namespace == "Ura.Data.Mappings");

            mapper.AddMappings(types);

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }
    }
}