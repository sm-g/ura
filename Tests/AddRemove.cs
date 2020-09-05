using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Ura.Models;

namespace Ura.Tests
{
    [TestClass]
    public class AddRemove
    {
        static FakeDataGetter dg;
        static Controller controller;

        private User u1, u2, u3;
        private Role r1, r2, r3;
        private Ability a1, a2, a3, a4;

        [TestInitialize]
        public void Init()
        {
            dg = new FakeDataGetter();
            controller = new Controller(dg);
            a1 = dg.a1;
            a2 = dg.a2;
            a3 = dg.a3;
            a4 = dg.a4;

            r1 = dg.r1;
            r2 = dg.r2;
            r3 = dg.r3;

            u1 = dg.u1;
            u2 = dg.u2;
            u3 = dg.u3;
        }

        /// <summary>
        /// Добавляем возможность в роль — она появляется у пользователей роли.
        /// </summary>
        [TestMethod]
        public void TestAddAtoR()
        {
            controller.AddRole(u1, r1);
            controller.AddAbility(r1, a1);
            Assert.IsTrue(controller.GetUserAbilities(u1).Single() == a1);
        }
        /// <summary>
        /// Добавляем возможность в роль — она не появляется у пользователей роли, если была запрещена.
        /// </summary>
        [TestMethod]
        public void TestAddAtoR2()
        {
            controller.AddAbility(r1, a1);
            controller.AddAbility(r1, a2);
            controller.AddRole(u1, r1);
            controller.AddRole(u1, r2);
            controller.RemoveAbility(u1, a1);
            controller.AddAbility(r2, a1);

            Assert.IsTrue(controller.GetUserAbilities(u1).Single() == a2);
        }
        /// <summary>
        /// Убираем возможность у роли — она пропадает у пользователей с этой ролью, если нет других ролей с ней.
        /// </summary>
        [TestMethod]
        public void TestRemoveAFromR()
        {
            controller.AddAbility(r1, a1);
            controller.AddRole(u1, r1);
            controller.RemoveAbility(r1, a1);
            Assert.IsTrue(controller.GetUserAbilities(u1).Count() == 0);
        }

        /// <summary>
        /// Убираем возможность у роли — она пропадает у пользователей с этой ролью, если нет других ролей с возможностью.
        /// </summary>
        [TestMethod]
        public void TestRemoveAFromR2()
        {
            // воможность сначала бонусом, потом через роль
            controller.AddAbility(u2, a1);
            controller.AddAbility(r2, a1);
            controller.AddRole(u2, r2);
            Assert.IsTrue(controller.GetUserAbilities(u2).Single() == a1);
            controller.RemoveAbility(r2, a1);
            Assert.IsTrue(controller.GetUserAbilities(u2).Count() == 0);
        }

        /// <summary>
        /// Убираем возможность у роли — она пропадает у пользователей с этой ролью, если нет других ролей с возможностью.
        /// </summary>
        [TestMethod]
        public void TestRemoveAFromR3()
        {
            // другая роль с возможностью
            controller.AddAbility(r2, a1);
            controller.AddAbility(r2, a2);
            controller.AddAbility(r1, a1);
            controller.AddRole(u2, r2);
            controller.AddRole(u2, r1);
            controller.RemoveAbility(r2, a1);
            Assert.IsTrue(controller.GetUserAbilities(u2).Count() == 2);
        }
        /// <summary>
        /// Когда все возможности в роли убраны, роль остается у пользователя.
        /// </summary>
        [TestMethod]
        public void TestRemoveAFromR4()
        {
            controller.AddAbility(r1, a1);
            controller.AddRole(u1, r1);
            controller.RemoveAbility(r1, a1);
            Assert.IsTrue(controller.GetUserRolesReal(u1).Count() == 1);
        }

        /// <summary>
        /// Добавляем роль пользователю — её возможности появляются у пользователя, если не были запрещены.
        /// </summary>
        [TestMethod]
        public void TestAddRtoU()
        {
            controller.AddAbility(r1, a1);
            controller.AddRole(u1, r1);
            Assert.IsTrue(controller.GetUserAbilities(u1).Single() == a1);
        }
        /// <summary>
        /// При добавлении роли пользователю из редактора ролей, добавляются только выбранные возможности роли.
        /// </summary>
        [TestMethod]
        public void TestAddRtoU2()
        {
            controller.AddAbility(r1, a1);
            controller.AddAbility(r1, a2);
            controller.AddAbility(r1, a3);

            controller.AddAbility(r3, a3);

            controller.AddRole(u1, r3);

            controller.AddRoleByAbility(u1, r1, new[] { a1 });
            Assert.IsTrue(controller.GetUserAbilities(u1).Count() == 2);
            Assert.IsTrue(controller.GetUserRolesReal(u1).Count() == 2);
        }
        /// <summary>
        /// При добавлении роли пользователю, запрещеные возможности не добавляются.
        /// </summary>
        [TestMethod]
        public void TestAddRtoU3()
        {
            controller.AddAbility(r1, a1);
            controller.AddAbility(r1, a2);
            controller.AddRole(u1, r1);
            controller.RemoveAbility(u1, a1);

            Assert.IsTrue(controller.GetUserAbilities(u1).Single() == a2);

            controller.AddAbility(r2, a1);
            controller.AddRole(u1, r2);
            Assert.IsTrue(controller.GetUserAbilities(u1).Single() == a2);
        }
        /// <summary>
        /// При добавлении роли пользователю из редактора ролей, возможности пользователя не меняются.
        /// </summary>
        [TestMethod]
        public void TestAddRtoU4()
        {
            controller.AddAbility(r1, a1);
            controller.AddAbility(r1, a2);
            controller.AddAbility(r1, a3);

            controller.AddAbility(u1, a1);

            controller.AddRoleByAbility(u1, r1, new[] { a1, a2 });

            Assert.IsTrue(controller.GetUserAbilities(u1).Single() == a1);
        }

        /// <summary>
        /// Убираем роль пользователю — возможности роли остаются, только если есть в других ролях.
        /// </summary>
        [TestMethod]
        public void TestRemoveRFromU()
        {
            // возможность через роль
            controller.AddAbility(r1, a1);
            controller.AddRole(u1, r1);
            controller.RemoveRole(u1, r1);
            Assert.IsTrue(controller.GetUserAbilities(u1).Count() == 0);
        }

        /// <summary>
        /// Убираем роль пользователю — возможности роли остаются, только если есть в других ролях.
        /// </summary>
        [TestMethod]
        public void TestRemoveRFromU2()
        {
            //  воможность сначала бонусом, потом через роль
            controller.AddAbility(u2, a1);
            controller.AddAbility(r2, a1);
            controller.AddRole(u2, r2);
            controller.RemoveRole(u2, r2);
            Assert.IsTrue(controller.GetUserAbilities(u2).Count() == 0);
        }
        /// <summary>
        /// Убираем роль пользователю — возможности роли остаются, только если есть в других ролях.
        /// </summary>
        [TestMethod]
        public void TestRemoveRFromU3()
        {
            // воможность есть в другой роли
            controller.AddAbility(r1, a1);
            controller.AddAbility(r1, a2);
            controller.AddAbility(r2, a1);

            controller.AddRole(u3, r2);
            controller.AddRole(u3, r1);
            Assert.IsTrue(controller.GetUserAbilities(u3).Count() == 2);
            controller.RemoveRole(u3, r1);
            Assert.IsTrue(controller.GetUserAbilities(u3).Single() == a1);
        }
        /// <summary>
        /// Если снова добавить роль пользователи, запрещенные возможности появляются у пользователя.
        /// </summary>
        [TestMethod]
        public void TestChangeUserRoles()
        {
            controller.AddAbility(r1, a1);
            controller.AddAbility(r1, a2);
            controller.AddRole(u1, r1);
            controller.RemoveAbility(u1, a1);
            Assert.IsTrue(controller.GetUserAbilities(u1).Single() == a2);

            controller.RemoveRole(u1, r1);
            Assert.IsTrue(controller.GetUserAbilities(u1).Count() == 0);

            controller.AddRole(u1, r1);
            Assert.IsTrue(controller.GetUserAbilities(u1).Count() == 2);
        }

        /// <summary>
        /// При удалении роли пользователю из редактора ролей пропадают возможности, которые только в этой роли и не были выбраны.
        /// </summary>
        [TestMethod]
        public void TestRemoveRFromU4()
        {
            controller.AddAbility(r1, a1);
            controller.AddAbility(r1, a2);
            controller.AddAbility(r1, a3);
            controller.AddAbility(r3, a3);

            controller.AddRole(u1, r3);
            controller.AddRole(u1, r1);

            // выбрали a1, a3 есть в другой роли
            controller.RemoveRoleByAbility(u1, r1, new[] { a1 });
            Assert.IsTrue(controller.GetUserAbilities(u1).Count() == 2);
            Assert.IsTrue(controller.GetUserAbilities(u1).Contains(a1));
            Assert.IsTrue(controller.GetUserAbilities(u1).Contains(a3));
            Assert.IsTrue(controller.GetUserRolesReal(u1).Single() == r3);

            controller.RemoveRoleByAbility(u1, r3, new[] { a3 });
            Assert.IsTrue(controller.GetUserAbilities(u1).Count() == 2);
            Assert.IsTrue(controller.GetUserAbilities(u1).Contains(a1));
            Assert.IsTrue(controller.GetUserAbilities(u1).Contains(a3));
            Assert.IsTrue(controller.GetUserRolesReal(u1).Count() == 0);
        }
        /// <summary>
        /// При удалении роли пользователю из редактора ролей возможности пользователя не меняются.
        /// </summary>
        [TestMethod]
        public void TestRemoveRFromU5()
        {
            controller.AddAbility(r1, a1);
            controller.AddAbility(r1, a2);
            controller.AddAbility(r1, a3);
            controller.AddAbility(r3, a3);

            controller.AddRole(u1, r3);
            controller.AddRole(u1, r1);

            // выбрали a1, a3 есть в другой роли
            controller.RemoveRoleByAbility(u1, r1, new[] { a1 });
            Assert.IsTrue(controller.GetUserAbilities(u1).Count() == 3);
            Assert.IsTrue(controller.GetUserAbilities(u1).Contains(a1));
            Assert.IsTrue(controller.GetUserAbilities(u1).Contains(a3));
            Assert.IsTrue(controller.GetUserRolesReal(u1).Single() == r3);
        }

        /// <summary>
        /// Меняем возможности пользователю, которые есть в роли.
        /// </summary>
        [TestMethod]
        public void TestChangeUserAbilities()
        {
            // роли автоматически добавляются
            controller.AddAbility(r1, a1);
            controller.AddAbility(u1, a1);
            Assert.IsTrue(controller.GetUserRolesReal(u1).Count() == 1);

            // роли пользователя не пропадают, когда все возможности роли убраны у пользователя
            controller.AddRole(u1, r1);
            controller.RemoveAbility(u1, a1);
            Assert.IsTrue(controller.GetUserRolesReal(u1).Count() == 1);
        }

        [TestMethod]
        public void TestMuliplyAdding()
        {
            controller.AddAbility(u1, a1);
            controller.AddAbility(u1, a1);

            controller.AddAbility(r1, a1);
            controller.AddAbility(r1, a1);

            controller.AddRole(u1, r1);
            controller.AddRole(u1, r1);

            Assert.IsTrue(controller.GetUserAbilities(u1).Count() == 1);
            Assert.IsTrue(controller.GetUserRolesReal(u1).Count() == 1);
        }
    }
}