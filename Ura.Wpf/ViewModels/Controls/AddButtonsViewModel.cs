using EventAggregator;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Ura.ViewModels.Screens;

namespace Ura.ViewModels
{
    public class AddButtonsViewModel<T> : ViewModelBase where T : class
    {
        private T _entity;
        private ICommand _add1;
        private ICommand _add2;
        private string _add1title;
        private string _add2title;
        private Func<SelectorViewModel> add1;
        private Func<SelectorViewModel> add2;

        public T Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                if (_entity != value)
                {
                    _entity = value;
                    OnPropertyChanged(() => Entity);
                }
            }
        }

        public ICommand Add1Command
        {
            get
            {
                return _add1
                   ?? (_add1 = new RelayCommand(() =>
                        {
                            this.Send(
                                (int)Event.OpenSelector,
                                new KeyValuePair<string, object>(Messenger.Selector, add1()));
                        }, () => Entity != null));
            }
        }

        public string Add1Title
        {
            get
            {
                return _add1title;
            }
            set
            {
                if (_add1title != value)
                {
                    _add1title = value;
                    OnPropertyChanged(() => Add1Title);
                }
            }
        }

        public ICommand Add2Command
        {
            get
            {
                return _add2
                   ?? (_add2 = new RelayCommand(() =>
                        {
                            this.Send(
                                (int)Event.OpenSelector,
                                new KeyValuePair<string, object>(Messenger.Selector, add2()));
                        }, () => Entity != null));
            }
        }

        public string Add2Title
        {
            get
            {
                return _add2title;
            }
            set
            {
                if (_add2title != value)
                {
                    _add2title = value;
                    OnPropertyChanged(() => Add2Title);
                }
            }
        }

        public AddButtonsViewModel(Func<SelectorViewModel> add1, string title1, Func<SelectorViewModel> add2, string title2)
        {
            this.add1 = add1;
            this.add2 = add2;
            this.Add1Title = title1;
            this.Add2Title = title2;
        }
    }
}