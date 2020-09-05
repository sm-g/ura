using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using Ura.Data;
using Ura.Models;
using NHibernate.Linq;

namespace Ura
{
    public enum Mode
    {
        Simple,
        WithSelected,
        FixedAbilities,
        Combo
    }
}

namespace Ura.ViewModels.Screens
{
    public abstract class AbstractUraScreenViewModel<T> : DialogViewModel where T : EntityBaseVM
    {
        protected Controller controller;
        private readonly ISession session;
        private ITransaction transaction;
        private ListViewModel list1;
        private ListViewModel list2;
        private Mode _mode;
        private bool _disposed;

        public AbstractUraScreenViewModel()
        {
            session = NHibernateHelper.OpenSession();
            transaction = session.BeginTransaction();

            controller = new Controller(new DataGetter(session));

            controller.ModelChanged += (s, e) => { CanApply = true; };
            CanOk = true;

            Mode = Ura.Mode.WithSelected;
        }

        /// <summary>
        /// Режим работы интерфейса
        /// </summary>
        public Mode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    OnPropertyChanged(() => Mode);
                }
            }
        }

        public EditorViewModel<T> Editor { get; protected set; }

        public T CurrentEntity
        {
            get { return Editor.SelectedEntity; }
        }

        public AddButtonsViewModel<T> Buttons { get; protected set; }

        public ListViewModel List1
        {
            get
            {
                return list1;
            }
            protected set
            {
                if (list1 != value)
                {
                    list1 = value;
                    OnPropertyChanged(() => List1);
                }
            }
        }

        public ListViewModel List2
        {
            get
            {
                return list2;
            }
            protected set
            {
                if (list2 != value)
                {
                    list2 = value;
                    OnPropertyChanged(() => List2);
                }
            }
        }

        protected void AfterConstructed()
        {
            RefreshTitle();
            Editor.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SelectedEntity")
                {
                    Buttons.Entity = CurrentEntity;
                    RefreshTitle();
                }
            };
            Editor.AllEntities.CollectionChanged += (s, e) => { CanApply = true; };
        }

        protected override void OnOk()
        {
            SaveToDB();
        }

        protected override void OnApply()
        {
            SaveToDB();

            transaction = session.BeginTransaction();

            controller.Refresh();
            CanApply = false;
        }

        protected override void OnCancel()
        {
            transaction.Rollback();
        }

        protected T CreateVM(IDeletable entity)
        {
            var vm = (T)Activator.CreateInstance(typeof(T), new[] { entity });
            vm.DeprecatedChanged += (s, e) =>
            {
                if (e.value)
                    controller.Delete(entity);
                else
                    controller.Restore(entity);
            };
            vm.PropertyChanged += (s, e) =>
            {
                CanOk = true;
                RefreshTitle();
            };
            return vm;
        }

        protected abstract void RefreshTitle();

        private void SaveToDB()
        {
            controller.PrepareSavingUsers();

            // новые сущности сохраняем
            List<object> unsaved = new List<object>();
            unsaved.AddRange(session.Query<User>().Where(x => x.Id == 0));
            unsaved.AddRange(session.Query<Role>().Where(x => x.Id == 0));
            unsaved.AddRange(session.Query<Ability>().Where(x => x.Id == 0));
            foreach (var item in unsaved)
            {
                session.Save(item);
            }

            transaction.Commit();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    session.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}