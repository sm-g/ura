using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Ura.ViewModels
{
    public class FilterViewModel<T> : ViewModelBase where T : IFiltrated
    {
        private readonly IEnumerable<T> collection;

        private bool _canFilter;
        private ICommand _filterCommand;
        private string _query;
        private bool _resultsOnQueryChanges;

        public string Query
        {
            get
            {
                return _query;
            }
            set
            {
                if (_query != value)
                {
                    _query = value;
                    OnPropertyChanged(() => Query);
                    if (UpdateResultsOnQueryChanges)
                    {
                        Filter();
                    }
                }
            }
        }

        public ObservableCollection<T> Results { get; private set; }

        public bool UpdateResultsOnQueryChanges
        {
            get
            {
                return _resultsOnQueryChanges;
            }
            set
            {
                if (_resultsOnQueryChanges != value)
                {
                    _resultsOnQueryChanges = value;
                    OnPropertyChanged("UpdateResultsOnQueryChanges");
                }
            }
        }

        public ICommand FilterCommand
        {
            get
            {
                return _filterCommand
                   ?? (_filterCommand = new RelayCommand(Filter, () => CanFilter));
            }
        }

        public bool CanFilter
        {
            get
            {
                return _canFilter;
            }
            set
            {
                if (_canFilter != value)
                {
                    _canFilter = value;
                    OnPropertyChanged(() => CanFilter);
                }
            }
        }

        public FilterViewModel(IEnumerable<T> collection, bool showAllAtStart)
        {
            this.collection = collection;
            CanFilter = true;

            if (showAllAtStart)
            {
                Results = new ObservableCollection<T>(collection);
            }
            else
            {
                Results = new ObservableCollection<T>();
            }
        }

        public void Filter()
        {
            if (!CanFilter)
                return;

            var res = collection.Where(i => i.Filter(Query));

            foreach (var item in Results.Except(res).ToList())
            {
                Results.Remove(item);
            }
            foreach (var item in res.Except(Results).ToList())
            {
                Results.Add(item);
            }
        }
    }
}