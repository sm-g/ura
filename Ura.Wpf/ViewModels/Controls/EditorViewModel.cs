using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ura.Models;

namespace Ura.ViewModels
{
    public class EditorViewModel<T> : ViewModelBase where T : EntityBaseVM, IDeletable
    {
        private string _prompt;
        private T _selected;
        private ICommand _new;
        private ICommand _delete;
        private Func<T> OnNewEntity;

        public ObservableCollection<T> AllEntities { get; private set; }

        public T SelectedEntity
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                OnPropertyChanged(() => SelectedEntity);
            }
        }

        public string Prompt
        {
            get
            {
                return _prompt;
            }
            set
            {
                if (_prompt != value)
                {
                    _prompt = value;
                    OnPropertyChanged(() => Prompt);
                }
            }
        }

        public ICommand NewEntityCommand
        {
            get
            {
                return _new
                   ?? (_new = new RelayCommand(() =>
                        {
                            var n = OnNewEntity();
                            AllEntities.Add(n);
                            SelectedEntity = n;
                        }));
            }
        }

        public ICommand DeleteEntityCommand
        {
            get
            {
                return _delete
                   ?? (_delete = new RelayCommand(() =>
                        {
                            SelectedEntity.Deprecated = true;
                        }, () => SelectedEntity != null && !SelectedEntity.Deprecated));
            }
        }

        public EditorViewModel(IEnumerable<T> all, string prompt, Func<T> onNewEntity)
        {
            AllEntities = new ObservableCollection<T>(all);
            Prompt = prompt;
            OnNewEntity = onNewEntity;
        }
    }
}