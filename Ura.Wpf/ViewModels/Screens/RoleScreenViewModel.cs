using System;
using System.Collections.Generic;
using System.Linq;

namespace Ura.ViewModels.Screens
{
    public class RoleScreenViewModel : AbstractUraScreenViewModel<RoleVM>
    {
        public IEnumerable<AbilityVM> SelectedAbilitiesVM
        {
            get { return List1 != null ? List1.SelectedItems.Cast<AbilityVM>() : Enumerable.Empty<AbilityVM>(); }
        }

        public RoleScreenViewModel()
        {
            Editor = new EditorViewModel<RoleVM>(controller.Roles.Select(r => CreateVM(r)),
                "Выберите роль:",
                () =>
                {
                    var ent = controller.CreateRole();
                    return CreateVM(ent);
                });
            Buttons = new AddButtonsViewModel<RoleVM>(OnAddAbilities, "+ возможности", OnAddUsers, "+ пользователи");
            Editor.PropertyChanged += Editor_PropertyChanged;
            controller.UserRolesChanged += (s, e1) =>
            {
                SetupAbilityUsers();
            };
            controller.UserAbilitiesChanged += (s, e1) =>
            {
                SetupAbilityUsers();
            };
            controller.RoleAbilitiesChanged += (s, e1) =>
            {
                SetupRoleAbilities();
                SetupAbilityUsers();
            };
            controller.DeprecatedChanged += (s, e1) =>
            {
                if (e1.entity == CurrentEntity.role)
                    CurrentEntity.OnDeprecatedChangedByCode();
            };
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Mode")
                {
                    SetupAbilityUsers();
                }
            };

            AfterConstructed();
        }

        protected override void RefreshTitle()
        {
            if (CurrentEntity != null)
            {
                Title = string.Format("Редактирование роли «{0}»", CurrentEntity);
            }
            else
            {
                Title = "Редактирование ролей";
            }
        }

        private SelectorViewModel OnAddAbilities()
        {
            var freeAbilitiesVM = controller.Abilities.OnlyActive()
                .Except(CurrentEntity.role.Abilities)
                .Select(a => new AbilityVM(a));
            Action<IEnumerable<EntityBaseVM>> handler = (selectedItems) =>
            {
                foreach (AbilityVM aVM in selectedItems.Cast<AbilityVM>())
                {
                    controller.AddAbility(CurrentEntity.role, aVM.ability);
                }
            };
            return new SelectorViewModel(freeAbilitiesVM, handler)
            {
                Title = string.Format("Добавление возможностей в роль «{0}»", CurrentEntity)
            };
        }

        private SelectorViewModel OnAddUsers()
        {
            var freeUsersVM = controller.GetUsersWithoutRole(CurrentEntity.role).OnlyActive()
                .Select(u => new UserVM(u));
            Action<IEnumerable<EntityBaseVM>> handler = (selectedItems) =>
            {
                foreach (UserVM uVM in selectedItems.Cast<UserVM>())
                {
                    controller.AddRole(uVM.user, CurrentEntity.role);
                }
            };
            return new SelectorViewModel(freeUsersVM, handler)
            {
                Title = string.Format("Добавление роли «{0}» пользователям", CurrentEntity)
            };
        }

        private void Editor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedEntity")
            {
                // выбрали другую роль
                SetupRoleAbilities();
                SetupAbilityUsers();
            }
        }

        private void List1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItems")
            {
                // выбрали возможности
                SetupAbilityUsers();
            }
        }

        private void SetupRoleAbilities()
        {
            if (CurrentEntity == null)
            {
                List1 = null;
                return;
            }
            var roleAbilitiesVM = CurrentEntity.role.Abilities
                .Select(a => new AbilityVM(a) { IsChecked = true })
                .ToList();
            List1 = new ListViewModel(roleAbilitiesVM);
            List1.OnlyDelete = true;
            List1.Title = "Возможности роли";
            List1.PropertyChanged += List1_PropertyChanged;
            foreach (var a in roleAbilitiesVM)
            {
                a.PropertyChanged += aVM_PropertyChanged;
            }
        }

        private void SetupAbilityUsers()
        {
            if (CurrentEntity == null)
            {
                List2 = null;
                return;
            }
            var selAbilities = SelectedAbilitiesVM.ToList();
            List<UserVM> visUsersVM;
            string title;
            if (selAbilities.Count > 0)
            {
                switch (Mode)
                {
                    case Mode.Simple:
                        // пользователи роли, у которых есть любая из выбранных возможностей
                        visUsersVM = controller.GetUsersWithRole(CurrentEntity.role)
                            .Where(u => selAbilities.Any(a => controller.GetUserAbilities(u).Contains(a.ability)))
                            .Select(u => new UserVM(u)).ToList();
                        break;

                    default:
                        //  пользователи с любой из выбранных возможностей
                        visUsersVM = selAbilities
                            .SelectMany(a => controller.GetUsersWithAbility(a.ability)).Distinct()
                            .Select(u => new UserVM(u)).ToList();
                        break;
                }

                title = selAbilities.Count == 1 ? string.Format("Пользователи с возможностью «{0}»", selAbilities.First()) : "Пользователи с выбранными возможностями";
            }
            else
            {
                visUsersVM = controller.GetUsersWithRole(CurrentEntity.role)
                    .Select(u => new UserVM(u)).ToList();
                title = "Все пользователи с ролью";
            }
            List2 = new ListViewModel(visUsersVM)
            {
                Title = title,
                OnlyDelete = Mode == Mode.Simple
            };
            foreach (var u in visUsersVM)
            {
                switch (Mode)
                {
                    case Mode.Simple:
                        u.IsChecked = true;
                        break;

                    default:
                        if (u.user.Roles.Contains(CurrentEntity.role))
                        {
                            if (selAbilities.All(a => controller.GetUserAbilities(u.user).Contains(a.ability)))
                                // у пользователя все выбранные возможности
                                u.IsChecked = true;
                            else
                                // если есть не все из выбранных возможностей
                                u.IsChecked = null;
                        }
                        else
                        {
                            u.IsChecked = false;
                        }
                        break;
                }

                u.PropertyChanged += userVM_PropertyChanged;
            }
        }

        private void aVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var a = sender as AbilityVM;
            if (e.PropertyName == "IsChecked")
            {
                // изменили флаг возможности роли
                if (a.IsChecked.HasValue && a.IsChecked.Value)
                {
                    controller.AddAbility(CurrentEntity.role, a.ability);
                }
                else
                {
                    controller.RemoveAbility(CurrentEntity.role, a.ability);
                }
            }
        }

        private void userVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var u = sender as UserVM;
            if (e.PropertyName == "IsChecked")
            {
                // изменили флаг роли пользователю

                switch (Mode)
                {
                    case Mode.Simple:
                        controller.RemoveRole(u.user, CurrentEntity.role);
                        break;

                    case Mode.WithSelected:
                        if (u.IsChecked.HasValue && u.IsChecked.Value)
                        {
                            controller.AddRoleByAbility(u.user, CurrentEntity.role, SelectedAbilitiesVM.Select(a => a.ability));
                        }
                        else
                        {
                            controller.RemoveRoleByAbility(u.user, CurrentEntity.role, SelectedAbilitiesVM.Select(a => a.ability));
                        }
                        break;

                    case Mode.FixedAbilities:
                        if (u.IsChecked.HasValue && u.IsChecked.Value)
                        {
                            controller.AddRoleSavingAbilities(u.user, CurrentEntity.role);
                        }
                        else
                        {
                            controller.RemoveRoleSavingAbilities(u.user, CurrentEntity.role);
                        }
                        break;

                    case Mode.Combo:
                        if (u.IsChecked.HasValue && u.IsChecked.Value)
                        {
                            controller.AddRole(u.user, CurrentEntity.role);
                        }
                        else
                        {
                            controller.RemoveRole(u.user, CurrentEntity.role);
                        }
                        break;
                }
            }
        }
    }
}