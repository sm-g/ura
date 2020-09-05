using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ura.ViewModels
{
    public class ListViewModel : ViewModelBase
    {
        private EntityBaseVM _selected;
        private string _title;
        private bool _onlyDelete;
        private bool _highlight;

        public ReadOnlyObservableCollection<EntityBaseVM> Items { get; private set; }

        public EntityBaseVM SelectedItem
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged(() => SelectedItem);
                }
            }
        }

        public IEnumerable<EntityBaseVM> SelectedItems { get { return Items.Where(i => i.IsSelected); } }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(() => Title);
                }
            }
        }

        public bool OnlyDelete
        {
            get
            {
                return _onlyDelete;
            }
            set
            {
                if (_onlyDelete != value)
                {
                    _onlyDelete = value;
                    OnPropertyChanged(() => OnlyDelete);
                }
            }
        }

        public bool HighlightUnchecked
        {
            get
            {
                return _highlight;
            }
            set
            {
                if (_highlight != value)
                {
                    _highlight = value;
                    OnPropertyChanged(() => HighlightUnchecked);
                }
            }
        }

        public ListViewModel(ObservableCollection<EntityBaseVM> items)
        {
            Items = new ReadOnlyObservableCollection<EntityBaseVM>(items);
        }

        public ListViewModel(IEnumerable<EntityBaseVM> items)
        {
            Items = new ReadOnlyObservableCollection<EntityBaseVM>(new ObservableCollection<EntityBaseVM>(items));
            foreach (var item in Items)
            {
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "IsSelected")
                    {
                        OnPropertyChanged(() => SelectedItems);
                    }
                };
            }
        }
    }
}