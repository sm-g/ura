using System;
using System.Collections.Generic;
using System.Linq;

namespace Ura.ViewModels.Screens
{
    public class UserScreenViewModel : AbstractUraScreenViewModel<UserVM>
    {
        public IEnumerable<RoleVM> SelectedRolesVM
        {
            get { return List1 != null ? List1.SelectedItems.Cast<RoleVM>() : Enumerable.Empty<RoleVM>(); }
        }

        public UserScreenViewModel()
        {
            Editor = new EditorViewModel<UserVM>(controller.Users.Select(u => CreateVM(u)),
                "Выберите пользователя:",
                () =>
                {
                    var ent = controller.CreateUser();
                    return CreateVM(ent);
                });
            Buttons = new AddButtonsViewModel<UserVM>(OnAddRoles, "+ роли", OnAddAbilities, "+ возможности");

            Editor.PropertyChanged += Editor_PropertyChanged;
            controller.UserRolesChanged += (s, e1) =>
            {
                SetupUserRoles();
                SetupRoleAbilities();
            };
            controller.UserAbilitiesChanged += (s, e1) =>
            {
                SetupRoleAbilities();
            };
            // возможности в роли не меняются
            controller.DeprecatedChanged += (s, e1) =>
            {
                if (e1.entity == CurrentEntity.user)
                    CurrentEntity.OnDeprecatedChangedByCode();
            };

            AfterConstructed();
        }

        protected override void RefreshTitle()
        {
            if (CurrentEntity != null)
            {
                Title = string.Format("Редактирование пользователя «{0}»", CurrentEntity);
            }
            else
            {
                Title = "Редактирование пользователей";
            }
        }

        private SelectorViewModel OnAddRoles()
        {
            var freeRolesVM = controller.Roles.OnlyActive()
                .Except(CurrentEntity.user.Roles)
                .Select(r => new RoleVM(r));
            Action<IEnumerable<EntityBaseVM>> handler = (selectedItems) =>
            {
                foreach (RoleVM roleVM in selectedItems.Cast<RoleVM>())
                {
                    controller.AddRole(CurrentEntity.user, roleVM.role);
                }
            };
            return new SelectorViewModel(freeRolesVM, handler)
             {
                 Title = string.Format("Добавление ролей пользователю «{0}»", CurrentEntity)
             };
        }

        private SelectorViewModel OnAddAbilities()
        {
            var freeAbilitiesVM = controller.Abilities.OnlyActive()
                .Except(controller.GetUserAbilities(CurrentEntity.user))
                .Select(a => new AbilityVM(a));
            Action<IEnumerable<EntityBaseVM>> handler = (selectedItems) =>
            {
                foreach (AbilityVM aVM in selectedItems.Cast<AbilityVM>())
                {
                    controller.AddAbility(CurrentEntity.user, aVM.ability);
                }
            };
            return new SelectorViewModel(freeAbilitiesVM, handler)
            {
                Title = string.Format("Добавление возможностей пользователю «{0}»", CurrentEntity)
            };
        }

        private void Editor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedEntity")
            {
                // выбрали другого пользователя
                SetupUserRoles();
                SetupRoleAbilities();
            }
        }

        private void List1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItems")
            {
                // выбрали роли
                SetupRoleAbilities();
            }
        }

        private void SetupUserRoles()
        {
            if (CurrentEntity == null)
            {
                List1 = null;
                return;
            }
            // depr
            var userRolesVM = CurrentEntity.user.Roles
                .Select(r => new RoleVM(r) { IsChecked = true })
                .ToList();
            List1 = new ListViewModel(userRolesVM);
            List1.OnlyDelete = true;
            List1.Title = "Роли пользователя";
            List1.PropertyChanged += List1_PropertyChanged;
            foreach (var r in userRolesVM)
            {
                r.PropertyChanged += roleVM_PropertyChanged;
            }
        }

        private void SetupRoleAbilities()
        {
            if (CurrentEntity == null)
            {
                List2 = null;
                return;
            }
            var selRoles = SelectedRolesVM.ToList();
            List<AbilityVM> visAbilitiesVM;
            string title;
            if (selRoles.Count > 0)
            {
                visAbilitiesVM = selRoles
                    .SelectMany(r => r.role.Abilities).Distinct()
                    .Select(a => new AbilityVM(a)).ToList();
                title = selRoles.Count == 1 ? string.Format("Возможности роли «{0}»", selRoles.First()) : "Возможности выбранных ролей";
            }
            else
            {
                // нет выбранных ролей — показываем все возможности
                visAbilitiesVM = controller.GetUserAbilities(CurrentEntity.user)
                    .Select(a => new AbilityVM(a)).ToList();
                title = "Все возможности пользователя";
            }
            List2 = new ListViewModel(visAbilitiesVM)
            {
                Title = title,
                HighlightUnchecked = true
            };
            foreach (var a in visAbilitiesVM)
            {
                // флаг показывает возможности роли у пользователя,
                // если выбрано несколько ролей — возможность у всех ролей или у некоторых
                if (controller.GetUserAbilities(CurrentEntity.user).Contains(a.ability))
                {
                    if (selRoles.All(r => r.role.Abilities.Contains(a.ability)))
                        a.IsChecked = true;
                    else
                        a.IsChecked = null;
                }
                else
                {
                    a.IsChecked = false;
                }

                a.PropertyChanged += abilityVM_PropertyChanged;
            }
        }

        private void roleVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var r = sender as RoleVM;
            if (e.PropertyName == "IsChecked")
            {
                // изменили флаг роли
                if (r.IsChecked.HasValue && r.IsChecked.Value)
                {
                    controller.AddRole(CurrentEntity.user, r.role);
                }
                else
                {
                    controller.RemoveRole(CurrentEntity.user, r.role);
                }
            }
        }

        private void abilityVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var a = sender as AbilityVM;
            if (e.PropertyName == "IsChecked")
            {
                // изменили флаг возможности
                if (a.IsChecked.HasValue && a.IsChecked.Value)
                    controller.AddAbility(CurrentEntity.user, a.ability);
                else
                    controller.RemoveAbility(CurrentEntity.user, a.ability);
            }
        }
    }
}