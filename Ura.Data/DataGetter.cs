using NHibernate;
using System.Collections.Generic;
using Ura.Models;

namespace Ura.Data
{
    public class DataGetter : IDataGetter
    {
        readonly ISession session;

        public IEnumerable<User> GetUsers()
        {
            return session.QueryOver<User>().List();
        }

        public IEnumerable<Role> GetRoles()
        {
            return session.QueryOver<Role>().List();
        }

        public IEnumerable<Ability> GetAbilities()
        {
            return session.QueryOver<Ability>().List();
        }

        public DataGetter(ISession session)
        {
            this.session = session;
        }
    }
}