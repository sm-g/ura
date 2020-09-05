using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ura.Data;
using Ura.Models;

namespace Ura
{
    public class Controller
    {
        private readonly Dictionary<User, bool> makingBonus = new Dictionary<User, bool>();
        private readonly IDataGetter dataGetter;
        private List<User> users;
        private List<Role> roles;
        private List<Ability> abilities;

        public event EventHandler<UserEventArgs> UserRolesChanged;

        public event EventHandler<RoleEventArgs> RoleAbilitiesChanged;

        public event EventHandler<UserEventArgs> UserAbilitiesChanged;

        public event EventHandler<DeletableEventArgs> DeprecatedChanged;

        public event EventHandler ModelChanged;

        public Controller(IDataGetter dg)
        {
            dataGetter = dg;
            Refresh();
        }

        public IEnumerable<User> Users { get { return users; } }

        public IEnumerable<Role> Roles { get { return roles; } }

        public IEnumerable<Ability> Abilities { get { return abilities; } }

        /// <summary>
        /// Указывает, что есть изменения, требующие сохранения. (Не требует сохранения создание бонусной роли.)
        /// </summary>
        public bool IsDirty
        {
            get;
            private set;
        }

        /// <summary>
        /// Убирает пользователям бонусную роль, сохраняя +возможности.
        /// </summary>
        public void PrepareSavingUsers()
        {
            foreach (var u in users)
            {
                BonusRole br = GetBonusRole(u);
                if (br != null)
                {
                    u.Roles.Remove(br);
                }
            }
        }

        /// <summary>
        /// Получает пользователей, роли и возможности из БД.
        /// </summary>
        public void Refresh()
        {
            users = new List<User>(dataGetter.GetUsers());
            // cоздаем бонусные роли пользователям
            foreach (var u in users)
            {
                MakeBonusRole(u);
            }

            roles = new List<Role>(dataGetter.GetRoles());
            abilities = new List<Ability>(dataGetter.GetAbilities());
        }

        public User CreateUser()
        {
            var u = new User();
            users.Add(u);
            return u;
        }

        public Role CreateRole()
        {
            var r = new Role();
            roles.Add(r);
            return r;
        }

        public Ability CreateAbility()
        {
            var a = new Ability();
            abilities.Add(a);
            return a;
        }

        #region Filters

        /// <summary>
        /// Все возможности = возможности из ролей + дополнительные - исключенные
        /// </summary>
        public ISet<Ability> GetUserAbilities(User u)
        {
            var _abilities = new HashSet<Ability>(u.Roles.SelectMany(r => r.Abilities));
            foreach (var a in GetExtraAbilities(u))
            {
                _abilities.Add(a);
            }
            foreach (var a in GetSubtractedAbilities(u))
            {
                _abilities.Remove(a);
            }

            return _abilities;
        }

        /// <summary>
        /// Действующие возможности у пользователя.
        /// </summary>
        public ISet<Ability> GetActiveUserAbilities(User u)
        {
            var _abilities = new HashSet<Ability>(u.Roles.OnlyActive().SelectMany(r => r.Abilities).OnlyActive());
            foreach (var a in GetExtraAbilities(u).OnlyActive())
            {
                _abilities.Add(a);
            }
            // запрещенные удаленные уже отфильтрованы
            return _abilities;
        }

        /// <summary>
        /// Реальные роли пользователя (без бонусной).
        /// </summary>
        public IEnumerable<Role> GetUserRolesReal(User u)
        {
            return u.Roles.Where(r => !(r is BonusRole));
        }

        public IEnumerable<User> GetUsersWithRole(Role r)
        {
            return users.Where(u => u.Roles.Contains(r));
        }

        public IEnumerable<User> GetUsersWithoutRole(Role r)
        {
            return users.Where(u => !u.Roles.Contains(r));
        }

        public IEnumerable<Role> GetRolesWithAbility(Ability a)
        {
            return roles.Where(r => r.Abilities.Contains(a));
        }

        public IEnumerable<Role> GetRolesWithoutAbility(Ability a)
        {
            return roles.Where(r => !r.Abilities.Contains(a));
        }

        public IEnumerable<User> GetUsersWithAbility(Ability a)
        {
            return users.Where(u => GetUserAbilities(u).Contains(a));
        }

        public IEnumerable<User> GetUsersWithoutAbility(Ability a)
        {
            return users.Where(u => !GetUserAbilities(u).Contains(a));
        }

        #endregion Filters

        #region AddRemove

        /// <summary>
        /// Добавляет роль пользователю. Все возможности роли переходят пользователю.
        /// </summary>
        public void AddRole(User u, Role r)
        {
            if (u.Roles.Add(r))
            {
                if (!makingBonus.GetBoolValueOrFalse(u))
                {
                    CheckExtraAbilities(u);
                    MakeBonusRole(u);
                }
                OnRolesChanged(u);
            }
        }

        /// <summary>
        /// Удаляет роль у пользователя. Возможности роли, которые есть в других ролях, остаются.
        /// </summary>
        public void RemoveRole(User u, Role r)
        {
            if (u.Roles.Remove(r))
            {
                if (r is BonusRole && !makingBonus.GetBoolValueOrFalse(u))
                {
                    foreach (var a in GetExtraAbilities(u))
                    {
                        u.OverAbilities.Remove(a);
                    }
                }
                else
                {
                    CheckSubtractedAbilities(u);
                }
                OnRolesChanged(u);
            }
        }

        /// <summary>
        /// Добавляет роль пользователю. Пользователю добавляются только выбранные возможности роли.
        /// </summary>
        public void AddRoleByAbility(User u, Role r, IEnumerable<Ability> selectedAbilities)
        {
            if (selectedAbilities.Any(a => !r.Abilities.Contains(a)))
                Debug.Fail("Выбрана возможность, которой нет в роли.");

            foreach (var a in r.Abilities)
            {
                // добавляем в исключения возможности роли, которых нет среди возможностей пользователя и выбранных
                if (!selectedAbilities.Contains(a) &&
                    !GetUserAbilities(u).Contains(a))
                {
                    u.OverAbilities[a] = false;
                }
            }

            AddRole(u, r);
        }

        /// <summary>
        /// Добавляет роль пользователю. Не меняет набор возможностей пользователя.
        /// </summary>
        public void AddRoleSavingAbilities(User u, Role r)
        {
            var before = GetUserAbilities(u);

            foreach (var a in r.Abilities)
            {
                // добавляем в исключения возможности роли, которых нет среди возможностей пользователя
                if (!GetUserAbilities(u).Contains(a))
                {
                    u.OverAbilities[a] = false;
                }
            }
            AddRole(u, r);

            var after = GetUserAbilities(u);
            if (!before.SetEquals(after))
                Debug.Fail(string.Format("{0} abilities changed", u));
        }

        /// <summary>
        /// Удаляет роль у пользователя. У пользователя остаются только выбранные возможности роли.
        /// </summary>
        public void RemoveRoleByAbility(User u, Role r, IEnumerable<Ability> selectedAbilities)
        {
            if (selectedAbilities.Any(a => !r.Abilities.Contains(a)))
                Debug.Fail("Выбрана возможность, которой нет в роли.");

            IEnumerable<Ability> abilities = selectedAbilities;

            foreach (var a in r.Abilities)
            {
                if (abilities.Contains(a))
                    u.OverAbilities[a] = true;
            }

            RemoveRole(u, r);
            // убираем лишние +возможности
            CheckExtraAbilities(u);
            MakeBonusRole(u);
        }

        /// <summary>
        /// Удаляет роль у пользователя. Не меняет набор возможностей пользователя.
        /// </summary>
        public void RemoveRoleSavingAbilities(User u, Role r)
        {
            var before = GetUserAbilities(u);

            IEnumerable<Ability> abilities = GetUserAbilities(u);

            foreach (var a in r.Abilities)
            {
                if (abilities.Contains(a))
                    u.OverAbilities[a] = true;
            }

            RemoveRole(u, r);
            // убираем лишние +возможности
            CheckExtraAbilities(u);
            MakeBonusRole(u);

            var after = GetUserAbilities(u);
            if (!before.SetEquals(after))
                Debug.Fail(string.Format("{0} abilities changed", u));
        }

        /// <summary>
        /// Добавляет возможность пользователю.
        /// </summary>
        public void AddAbility(User u, Ability a)
        {
            if (GetRolesAbilities(u).Contains(a) &&
                u.OverAbilities.ContainsKey(a) &&
                u.OverAbilities[a] == false)
            {
                // есть -возможность, убираем её
                u.OverAbilities.Remove(a);
                OnAbilitiesChanged(u);
            }
            else if (!GetRolesAbilities(u).Contains(a))
            {
                // такой возожности в ролях нет, добавляем +возможность
                u.OverAbilities[a] = true;

                // ищем и добавляем роль, все возможности которой есть среди +возможностей пользователя
                var roles = GetRolesWithAbility(a).ToList();
                var extras = GetExtraAbilities(u);
                bool roleFound = false;
                for (int i = 0; i < roles.Count && !roleFound; i++)
                {
                    if (roles[i].Abilities.All(ra => extras.Contains(ra)))
                    {
                        AddRole(u, roles[i]);
                        roleFound = true;
                    }
                }

                // если не нашли, возможность остаётся бонусной
                if (!roleFound)
                {
                    MakeBonusRole(u);
                    OnAbilitiesChanged(u);
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("возможность {0} уже есть у пользователя {1}", a, u));
            }
        }

        /// <summary>
        /// Убирает возможность пользователю.
        /// </summary>
        public void RemoveAbility(User u, Ability a)
        {
            if (GetRolesAbilities(u).Contains(a))
            {
                // возможность есть в какой-нибудь роли, добавляем -возможность
                u.OverAbilities[a] = false;
                OnAbilitiesChanged(u);
            }
            else if (u.OverAbilities[a])
            {
                // была +возможность, убираем
                u.OverAbilities.Remove(a);
                MakeBonusRole(u);
                OnAbilitiesChanged(u);
            }
            else
            {
                throw new InvalidOperationException(string.Format("у пользователя {0} не было возможности {1}", u, a));
            }
        }

        /// <summary>
        /// Добавляет возможность в роль. Все пользователи с ролью получают возможность, если она не была запрещена.
        /// </summary>
        public void AddAbility(Role r, Ability a)
        {
            if (r.Abilities.Add(a))
            {
                // убираем +a у пользователей с ролью r
                foreach (var u in GetUsersWithRole(r))
                {
                    CheckExtraAbilities(u);
                    MakeBonusRole(u);
                }

                OnAbilitiesChanged(r);
            }
        }

        /// <summary>
        /// Убирает возможность у роли. Пользователи с ролью теряют возможность, если нет других ролей с ней.
        /// </summary>
        public void RemoveAbility(Role r, Ability a)
        {
            if (r.Abilities.Remove(a))
            {
                foreach (var u in GetUsersWithRole(r))
                {
                    CheckSubtractedAbilities(u);
                }
                OnAbilitiesChanged(r);
            }
        }

        /// <summary>
        /// Добавляет возможность в роль. Возможность появляется у выбранных пользователей.
        /// У всех пользоватлеей уже есть добавляемая возможность, используем AddAbility(Role r, Ability a)
        /// </summary>
        //public void AddAbilityByUser(Role r, Ability a, IEnumerable<User> selectedUsers)
        //{
        //}

        /// <summary>
        /// Добавляет возможность в роль. Не меняет набор возможностей пользователей роли.
        /// </summary>
        public void AddAbilitySavingRoleUsersAbilities(Role r, Ability a)
        {
            // добавляемая возможность исключается у всех пользователей роли, если её не было у пользователя
            IEnumerable<User> users = GetUsersWithRole(r);

            if (r.Abilities.Add(a))
            {
                foreach (var u in users)
                {
                    if (!GetUserAbilities(u).Contains(a))
                        u.OverAbilities[a] = false;
                }
            }
            OnAbilitiesChanged(r);
        }

        /// <summary>
        /// Убирает возможность у роли. Возможность остаётся у выбранных пользователей с ролью.
        /// </summary>
        public void RemoveAbilityByUser(Role r, Ability a, IEnumerable<User> selectedUsers)
        {
            if (selectedUsers.Any(u => !GetUserAbilities(u).Contains(a)))
                Debug.Fail("Выбран пользователь, у которого нет возможности.");

            if (r.Abilities.Remove(a))
            {
                foreach (var u in selectedUsers)
                {
                    if (u.Roles.Contains(r) && !u.OverAbilities.ContainsKey(a))
                    {
                        u.OverAbilities[a] = true;
                        CheckExtraAbilities(u);
                        MakeBonusRole(u);
                    }
                }
            }

            OnAbilitiesChanged(r);
        }

        /// <summary>
        /// Убирает возможность у роли. Не меняет набор возможностей пользователей c ролью.
        /// </summary>
        public void RemoveAbilitySavingRoleUsersAbilities(Role r, Ability a)
        {
            if (r.Abilities.Remove(a))
            {
                // убранная возможность остается у всех пользователей роли, если не была исключена
                IEnumerable<User> users = GetUsersWithRole(r);

                foreach (var u in users)
                {
                    if (!u.OverAbilities.ContainsKey(a))
                    {
                        u.OverAbilities[a] = true;
                        CheckExtraAbilities(u);
                        MakeBonusRole(u);
                    }
                }
            }

            OnAbilitiesChanged(r);
        }

        #endregion AddRemove

        #region Deprecation

        /// <summary>
        /// Запрещает сущность.
        /// </summary>
        public void Delete(IDeletable e)
        {
            e.Deprecated = true;
            OnDeprChanged(e);
        }

        /// <summary>
        /// Разрешает сущность.
        /// </summary>
        public void Restore(IDeletable e)
        {
            e.Deprecated = false;
            OnDeprChanged(e);
        }

        /// <summary>
        /// Устанавливает состояние запрета пользователя.
        /// </summary>
        public void CheckDeprecated(User u)
        {
            var subtracted = GetSubtractedAbilities(u);
            // если у пользователя нет возможностей. Запрещены все +возможности и роли, возможности разрешенных ролей в -возможностях.
            var depr = GetUserRolesReal(u)
                    .OnlyActive()
                    .SelectMany(r => r.Abilities)
                    .All(a => subtracted.Contains(a))
                && GetExtraAbilities(u).All(a => a.Deprecated);

            if (depr && !u.Deprecated)
            {
                u.Deprecated = true;
                Delete(u);
            }
        }

        /// <summary>
        /// Устанавливает состояние запрета роли.
        /// </summary>
        public void CheckDeprecated(Role r)
        {
            // запрещена, если нет возможностей
            var depr = r.Abilities.All(a => a.Deprecated);
            var changed = r.Deprecated != depr;
            if (depr && !r.Deprecated)
            {
                r.Deprecated = true;
                Delete(r);
            }
        }

        private void CheckOnRoleDeprChanged(Role r)
        {
            // проверяем пользователей с ролью
            foreach (var u in GetUsersWithRole(r))
            {
                CheckDeprecated(u);
            }
        }

        private void ChecksOnAbilityDeprChanged(Ability a)
        {
            // проверяем пользователей с возможностью и роли, в которых она была
            foreach (var u in GetUsersWithAbility(a))
            {
                CheckDeprecated(u);
            }
            foreach (var r in GetRolesWithAbility(a))
            {
                CheckDeprecated(r);
            }
        }

        #endregion Deprecation

        public bool Can(User u, Ability a)
        {
            return GetUserAbilities(u).Contains(a);
        }

        private BonusRole GetBonusRole(User u)
        {
            return (BonusRole)u.Roles.SingleOrDefault(r => r is BonusRole);
        }

        /// <summary>
        /// Пересоздает бонусную роль из +возможностей, которых нет в ролях.
        /// </summary>
        private void MakeBonusRole(User u)
        {
            Debug.WriteLine("making bonus for {0}", u);
            makingBonus[u] = true;

            var extraAbilities = GetExtraAbilities(u);
            BonusRole br = GetBonusRole(u);
            if (br != null)
            {
                if (br.Abilities.SetEquals(extraAbilities))
                {
                    makingBonus[u] = false;
                    return;
                }
                RemoveRole(u, br);
            }

            if (extraAbilities.Count > 0)
            {
                AddRole(u, new BonusRole(extraAbilities));
            }
            makingBonus[u] = false;
        }

        /// <summary>
        /// Все возможности из ролей, кроме бонусной.
        /// </summary>
        private ISet<Ability> GetRolesAbilities(User user)
        {
            return new HashSet<Ability>(GetUserRolesReal(user)
                .SelectMany(r => r.Abilities)
                .Distinct());
        }

        private ISet<Ability> GetExtraAbilities(User u)
        {
            return new HashSet<Ability>(u.OverAbilities.Where(kvp => kvp.Value).Select(kvp => kvp.Key));
        }

        private ISet<Ability> GetSubtractedAbilities(User u)
        {
            return new HashSet<Ability>(u.OverAbilities.Where(kvp => !kvp.Value).Select(kvp => kvp.Key));
        }

        /// <summary>
        /// Убирает +возможности, которые есть в какой-нибудь роли пользователя. Требуется MakeBonusRole после.
        /// </summary>
        private void CheckExtraAbilities(User u)
        {
            foreach (var a in GetExtraAbilities(u))
            {
                if (GetRolesAbilities(u).Contains(a))
                {
                    u.OverAbilities.Remove(a);
                }
            }
        }

        /// <summary>
        /// Убирает -возможности, которых нет ни в одной роли пользователя.
        /// </summary>
        private void CheckSubtractedAbilities(User u)
        {
            foreach (var a in GetSubtractedAbilities(u))
            {
                if (!GetRolesAbilities(u).Contains(a))
                    u.OverAbilities.Remove(a);
            }
        }

        #region Events

        protected virtual void OnRolesChanged(User u)
        {
            Debug.WriteLine("{0} roles: {1}", u, u.Roles.Aggregate("", (s, r) => s + ' ' + r.ToString() + ';'));
            CheckDeprecated(u);

            var h = UserRolesChanged;
            if (h != null)
            {
                h(this, new UserEventArgs(u));
            }

            if (!makingBonus.GetBoolValueOrFalse(u)) // создание бонусной роли не меняет модель
                OnModelChanged();
        }

        protected virtual void OnAbilitiesChanged(Role r)
        {
            Debug.WriteLine("{0} abilities: {1}", r, r.Abilities.Aggregate("", (s, a) => s + ' ' + a.ToString() + ';'));
            CheckDeprecated(r);

            var h = RoleAbilitiesChanged;
            if (h != null)
            {
                h(this, new RoleEventArgs(r));
            }
            OnModelChanged();
        }

        protected virtual void OnAbilitiesChanged(User u)
        {
            Debug.WriteLine("{0} abilities: {1}", u, GetUserAbilities(u).Aggregate("", (s, a) => s + ' ' + a.ToString() + ';'));
            CheckDeprecated(u);

            var h = UserAbilitiesChanged;
            if (h != null)
            {
                h(this, new UserEventArgs(u));
            }
            OnModelChanged();
        }

        protected virtual void OnDeprChanged(IDeletable e)
        {
            Debug.WriteLine("{0} depr={1}", e, e.Deprecated);
            if (e is User)
            {
            }
            else if (e is Role)
            {
                CheckOnRoleDeprChanged(e as Role);
            }
            else if (e is Ability)
            {
                ChecksOnAbilityDeprChanged(e as Ability);
            }

            var h1 = DeprecatedChanged;
            if (h1 != null)
            {
                h1(this, new DeletableEventArgs(e));
            }
            OnModelChanged();
        }

        protected virtual void OnModelChanged()
        {
            IsDirty = true;
            var h1 = ModelChanged;
            if (h1 != null)
            {
                h1(this, EventArgs.Empty);
            }
        }

        #endregion Events
    }

    public class AbilityEventArgs : EventArgs
    {
        public Ability ability;

        [DebuggerStepThrough]
        public AbilityEventArgs(Ability a)
        {
            ability = a;
        }
    }

    public class RoleEventArgs : EventArgs
    {
        public Role role;

        [DebuggerStepThrough]
        public RoleEventArgs(Role r)
        {
            role = r;
        }
    }

    public class UserEventArgs : EventArgs
    {
        public User user;

        [DebuggerStepThrough]
        public UserEventArgs(User u)
        {
            user = u;
        }
    }
}