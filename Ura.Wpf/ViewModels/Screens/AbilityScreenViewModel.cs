using System;
using System.Collections.Generic;
using System.Linq;

namespace Ura.ViewModels.Screens
{
    public class AbilityScreenViewModel : AbstractUraScreenViewModel<AbilityVM>
    {
        public IEnumerable<UserVM> SelectedUsersVM
        {
            get { return List1 != null ? List1.SelectedItems.Cast<UserVM>() : Enumerable.Empty<UserVM>(); }
        }

        public AbilityScreenViewModel()
        {
            Editor = new EditorViewModel<AbilityVM>(controller.Abilities.Select(r => CreateVM(r)),
                "Выберите возможность:",
                () =>
                {
                    var ent = controller.CreateAbility();
                    return CreateVM(ent);
                });
            Buttons = new AddButtonsViewModel<AbilityVM>(OnAddUsers, "+ пользователи", OnAddRoles, "+ роли");
            Editor.PropertyChanged += Editor_PropertyChanged;

            controller.UserRolesChanged += (s, e1) =>
            {
                SetupAbilityUsers(); // на случай автодобавления роли при добавлении возможности
                SetupUserRoles();
            };
            controller.UserAbilitiesChanged += (s, e1) =>
            {
                SetupAbilityUsers();
                SetupUserRoles();
            };
            controller.RoleAbilitiesChanged += (s, e1) =>
            {
                SetupAbilityUsers(); // если вариант проще
                SetupUserRoles();
            };
            controller.DeprecatedChanged += (s, e1) =>
            {
                if (e1.entity == CurrentEntity.ability)
                    CurrentEntity.OnDeprecatedChangedByCode();
            };
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Mode")
                {
                    SetupUserRoles();
                }
            };

            AfterConstructed();
        }

        protected override void RefreshTitle()
        {
            if (CurrentEntity != null)
            {
                Title = string.Format("Редактирование возможности «{0}»", CurrentEntity);
            }
            else
            {
                Title = "Редактирование возможностей";
            }
        }

        private SelectorViewModel OnAddUsers()
        {
            var freeUsersVM = controller.GetUsersWithoutAbility(CurrentEntity.ability).OnlyActive()
                .Select(u => new UserVM(u));
            Action<IEnumerable<EntityBaseVM>> handler = (selectedItems) =>
            {
                foreach (UserVM uVM in selectedItems.Cast<UserVM>())
                {
                    controller.AddAbility(uVM.user, CurrentEntity.ability);
                }
            };
            return new SelectorViewModel(freeUsersVM, handler)
            {
                Title = string.Format("Добавление возможности «{0}» пользователям", CurrentEntity)
            };
        }

        private SelectorViewModel OnAddRoles()
        {
            var freeRolesVM = controller.GetRolesWithoutAbility(CurrentEntity.ability).OnlyActive()
                .Select(r => new RoleVM(r));
            Action<IEnumerable<EntityBaseVM>> handler = (selectedItems) =>
            {
                foreach (RoleVM rVM in selectedItems.Cast<RoleVM>())
                {
                    controller.AddAbility(rVM.role, CurrentEntity.ability);
                }
            };
            return new SelectorViewModel(freeRolesVM, handler)
            {
                Title = string.Format("Добавление возможности «{0}» в роли", CurrentEntity)
            };
        }

        private void Editor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedEntity")
            {
                // выбрали другую возможность
                SetupAbilityUsers();
                SetupUserRoles();
            }
        }

        private void List1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItems")
            {
                // выбрали пользователей
                SetupUserRoles();
            }
        }

        private void SetupAbilityUsers()
        {
            if (CurrentEntity == null)
            {
                List1 = null;
                return;
            }

            var abilityUsersVM = controller.GetUsersWithAbility(CurrentEntity.ability)
                .Select(u => new UserVM(u) { IsChecked = true })
                .ToList();
            List1 = new ListViewModel(abilityUsersVM);
            List1.OnlyDelete = true;
            List1.Title = "Пользователи с возможностью";
            List1.PropertyChanged += List1_PropertyChanged;
            foreach (var a in abilityUsersVM)
            {
                a.PropertyChanged += userVM_PropertyChanged;
            }
        }

        private void SetupUserRoles()
        {
            if (CurrentEntity == null)
            {
                List2 = null;
                return;
            }
            var selUsers = SelectedUsersVM.ToList();
            List<RoleVM> visRolesVM;
            string title;
            if (selUsers.Count > 0)
            {
                switch (Mode)
                {
                    case Mode.Simple:
                        // роли с возможностью, которые есть у выбранных пользователей
                        visRolesVM = controller.GetRolesWithAbility(CurrentEntity.ability)
                            .Where(r => selUsers.Any(u => controller.GetUserRolesReal(u.user).Contains(r)))
                            .Select(r => new RoleVM(r)).ToList();
                        break;

                    default:
                        // роли  любого из выбранных пользователей
                        visRolesVM = selUsers
                            .SelectMany(u => controller.GetUserRolesReal(u.user)).Distinct()
                            .Select(r => new RoleVM(r)).ToList();
                        break;
                }

                title = selUsers.Count == 1 ? string.Format("Роли пользователя «{0}»", selUsers.First()) : "Роли выбранных пользователей";
            }
            else
            {
                visRolesVM = controller.GetRolesWithAbility(CurrentEntity.ability)
                    .Select(r => new RoleVM(r)).ToList();
                title = "Все роли с возможностью";
            }
            List2 = new ListViewModel(visRolesVM)
            {
                Title = title,
                OnlyDelete = Mode == Mode.Simple
            };
            foreach (var r in visRolesVM)
            {
                switch (Mode)
                {
                    case Mode.Simple:
                        r.IsChecked = true;
                        break;

                    default:
                        if (r.role.Abilities.Contains(CurrentEntity.ability))
                        {
                            if (selUsers.All(u => controller.GetUserRolesReal(u.user).Contains(r.role)))
                                // роль у всех выбранных пользователей
                                r.IsChecked = true;
                            else
                                r.IsChecked = null;
                        }
                        else
                        {
                            r.IsChecked = false;
                        }
                        break;
                }

                r.PropertyChanged += roleVM_PropertyChanged;
            }
        }

        private void userVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var u = sender as UserVM;
            if (e.PropertyName == "IsChecked")
            {
                // изменили флаг пользователю с возможностью
                if (u.IsChecked.HasValue && u.IsChecked.Value)
                {
                    controller.AddAbility(u.user, CurrentEntity.ability);
                }
                else
                {
                    controller.RemoveAbility(u.user, CurrentEntity.ability);
                }
            }
        }

        private void roleVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var r = sender as RoleVM;
            if (e.PropertyName == "IsChecked")
            {
                // изменили флаг возможности роли

                switch (Mode)
                {
                    case Mode.Simple:
                        controller.RemoveAbility(r.role, CurrentEntity.ability);
                        break;

                    case Mode.WithSelected:
                        if (r.IsChecked.HasValue && r.IsChecked.Value)
                        {
                            controller.AddAbility(r.role, CurrentEntity.ability);
                        }
                        else
                        {
                            controller.RemoveAbilityByUser(r.role, CurrentEntity.ability, SelectedUsersVM.Select(u => u.user));
                        }
                        break;

                    case Mode.FixedAbilities:
                        if (r.IsChecked.HasValue && r.IsChecked.Value)
                        {
                            controller.AddAbilitySavingRoleUsersAbilities(r.role, CurrentEntity.ability);
                        }
                        else
                        {
                            controller.RemoveAbilitySavingRoleUsersAbilities(r.role, CurrentEntity.ability);
                        }
                        break;

                    case Mode.Combo:
                        if (r.IsChecked.HasValue && r.IsChecked.Value)
                        {
                            controller.AddAbility(r.role, CurrentEntity.ability);
                        }
                        else
                        {
                            controller.RemoveAbility(r.role, CurrentEntity.ability);
                        }
                        break;
                }
            }
        }
    }
}