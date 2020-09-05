using System;
using Ura.Models;

namespace Ura.ViewModels
{
    public abstract class EntityBaseVM : ViewModelBase, IDeletable, IFiltrated
    {
        protected bool deprecated;
        private bool? _isChecked;
        private bool _isSelected;

        public event EventHandler<BoolEventArgs> DeprecatedChanged; // не используем PropertyChanged для отслеживания изменения Deprecated
        public bool? IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(() => IsChecked);
                }
            }
        }
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(() => IsSelected);
                }
            }
        }

        public abstract bool IsUnsaved { get; }

        public abstract bool Deprecated { get; set; }
        public abstract bool Filter(string query);
        public abstract string Represent { get; }

        public void OnDeprecatedChangedByCode()
        {
            OnPropertyChanged("Deprecated");
        }

        protected virtual void OnDeprChanged(bool value)
        {
            var h = DeprecatedChanged;
            if (h != null)
            {
                h(this, new BoolEventArgs(value));
            }
        }
    }

    public class BoolEventArgs : EventArgs
    {
        public bool value;
        public BoolEventArgs(bool value)
        {
            this.value = value;
        }
    }
}