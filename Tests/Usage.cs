using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ura.Models;
using System.Text;

namespace Ura.Tests
{
    [TestClass]
    public class Usage
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
        /// Проверяем, если ли у пользователя возможность печатать.
        /// </summary>
        [TestMethod]
        public void Example()
        {
            Ability print = new Ability() { Description = "print" };
            Role admin = new Role(new[] { print }) { Description = "admin" };
            User user = new User();
            var printer = new Printer();

            bool canPrint = controller.Can(user, print);
            Assert.IsFalse(canPrint); // пользователь не может печатать

            user.Roles.Add(admin); // добавляем ему роль «администратор», у которой такая возможность есть

            if (controller.Can(user, print))
            {
                printer.Print("document");
            }
            else
            {
                throw new Exception(string.Format("Пользователь {0} не может {1}", user, print));
            }
        }

        class Printer
        {
            public void Print(string str)
            {

            }
        }
    }
}
