using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ura.Data;
using Ura.Models;

namespace Ura.Tests
{
    class FakeDataGetter : IDataGetter
    {
        public Ability a1 = new Ability() { Id = 1, Description = "save" };
        public Ability a2 = new Ability() { Id = 2, Description = "update" };
        public Ability a3 = new Ability() { Id = 3, Description = "create" };
        public Ability a4 = new Ability() { Id = 4, Description = "delete" };

        public Role r1 = new Role() { Id = 1, Description = "root" };
        public Role r2 = new Role() { Id = 2, Description = "admin" };
        public Role r3 = new Role() { Id = 3, Description = "manager" };

        public User u1 = new User() { Id = 1, Login = "ivan" };
        public User u2 = new User() { Id = 2, Login = "dave" };
        public User u3 = new User() { Id = 3, Login = "asd" };

        public FakeDataGetter()
        {
            //u1.Roles.Add(r3);
            //u2.Roles.Add(r1);
            //u2.Roles.Add(r2);
            //u3.OverAbilities.Add(a1, true);
            //u1.OverAbilities.Add(a3, true);
            //u2.OverAbilities.Add(a1, false);
        }

        public IEnumerable<User> GetUsers()
        {
            return new[] {
                u1, u2, u3
            };
        }

        public IEnumerable<Role> GetRoles()
        {
            return new[] {
                r1, r2, r3
            };
        }

        public IEnumerable<Ability> GetAbilities()
        {
            return new[] {
               a1,a2, a3, a4
            };
        }
    }
}
