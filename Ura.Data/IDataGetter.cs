using System.Collections.Generic;
using Ura.Models;

namespace Ura.Data
{
    public interface IDataGetter
    {
        IEnumerable<Ability> GetAbilities();

        IEnumerable<Role> GetRoles();

        IEnumerable<User> GetUsers();
    }
}