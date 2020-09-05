using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ura.Models;
using System.Text;

namespace Ura.Tests
{
    [TestClass]
    public class Deprecated
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
        /// Пользователеь запрещен, если у него нет незапрещенных возможностей, то есть 
        /// запрещены роли, +возможности, а возможности незапрещенных ролей — −возможности.
        /// </summary>
        [TestMethod]
        public void TestUserState()
        {
            controller.CheckDeprecated(u1);
            Assert.IsTrue(u1.Deprecated);

            controller.AddRole(u2, r1);
            controller.CheckDeprecated(u2);
            Assert.IsTrue(u2.Deprecated);
        }

        /// <summary>
        /// Появляется возможность - пользователь становится не запрещен.
        /// </summary>
        [TestMethod]
        public void TestUserState2()
        {
            controller.CheckDeprecated(u1);
            controller.AddRole(u1, r1);
            controller.AddAbility(r1, a1);
            controller.CheckDeprecated(u1);
            Assert.IsFalse(u1.Deprecated);

            controller.CheckDeprecated(u2);
            controller.AddAbility(u2, a1);
            controller.CheckDeprecated(u2);
            Assert.IsFalse(u2.Deprecated);
        }
        /// <summary>
        /// Появляется возможность - пользователь остается запрещен.
        /// </summary>
        [TestMethod]
        public void TestUserState3()
        {
            controller.CheckDeprecated(u1);
            controller.AddRole(u1, r1);
            controller.AddAbility(r1, a1);
            controller.CheckDeprecated(u1);
            Assert.IsTrue(u1.Deprecated);

            controller.CheckDeprecated(u2);
            controller.AddAbility(u2, a1);
            controller.CheckDeprecated(u2);
            Assert.IsTrue(u2.Deprecated);
        }
        /// <summary>
        /// Убираем возможность
        /// </summary>
        [TestMethod]
        public void TestUserState4()
        {
            controller.AddAbility(r1, a1);
            controller.AddRole(u1, r1);
            controller.CheckDeprecated(u1);
            Assert.IsFalse(u1.Deprecated);

            controller.RemoveAbility(u1, a1);
            controller.CheckDeprecated(u1);
            Assert.IsTrue(u1.Deprecated);
        }

        /// <summary>
        /// Запрещеаем возможность
        /// </summary>
        [TestMethod]
        public void TestUserState5()
        {
            controller.AddAbility(u1, a3);
            controller.CheckDeprecated(u1);
            Assert.IsFalse(u1.Deprecated);

            a3.Deprecated = true;
            controller.CheckDeprecated(u1);
            Assert.IsTrue(u1.Deprecated);
        }

        /// <summary>
        /// Зарещеаем роль
        /// </summary>
        [TestMethod]
        public void TestUserState6()
        {
            controller.AddAbility(r1, a2);
            controller.AddRole(u1, r1);
            controller.CheckDeprecated(u1);
            Assert.IsFalse(u1.Deprecated);

            r1.Deprecated = true;
            controller.CheckDeprecated(u1);
            Assert.IsTrue(u1.Deprecated);
        }

        /// <summary>
        /// Роль запрещена, если у неё нет возможностей.
        /// </summary>
        [TestMethod]
        public void TestRoleState()
        {
            controller.CheckDeprecated(r1);
            Assert.IsTrue(r1.Deprecated);
        }

        /// <summary>
        /// Роль запрещена, если у неё нет незапрещенных возможностей.
        /// </summary>
        [TestMethod]
        public void TestRoleState2()
        {
            controller.AddAbility(r1, a1);
            controller.CheckDeprecated(r1);
            Assert.IsFalse(r1.Deprecated);

            a1.Deprecated = true;
            controller.CheckDeprecated(r1);
            Assert.IsTrue(r1.Deprecated);
        }

        /// <summary>
        /// Появляется возможность — роль остается запрещенной.
        /// </summary>
        [TestMethod]
        public void TestRoleState3()
        {
            controller.CheckDeprecated(r1);
            controller.AddAbility(r1, a1);
            Assert.IsTrue(r1.Deprecated);
        }
    }
}
