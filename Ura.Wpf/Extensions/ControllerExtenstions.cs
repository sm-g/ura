using System.Collections.Generic;
using System.Linq;
using Ura.Models;

namespace Ura
{
    public static class ControllerExtensions
    {
        public static IEnumerable<Role> OnlyActive(this IEnumerable<Role> roles)
        {
            return roles.Where(r => !r.Deprecated);
        }

        public static IEnumerable<User> OnlyActive(this IEnumerable<User> users)
        {
            return users.Where(u => !u.Deprecated);
        }

        public static IEnumerable<Ability> OnlyActive(this IEnumerable<Ability> abilities)
        {
            return abilities.Where(a => !a.Deprecated);
        }
    }
}