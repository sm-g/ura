using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ura.ViewModels.Screens
{
    public class SelectorViewModel : DialogViewModel
    {
        private Action<IEnumerable<EntityBaseVM>> onOk;
        private FilterViewModel<EntityBaseVM> filter;

        public ObservableCollection<EntityBaseVM> Items { get { return filter.Results; } }

        public IEnumerable<EntityBaseVM> SelectedItems { get { return Items.Where(i => i.IsSelected); } }

        public string Query
        {
            get
            {
                return filter.Query;
            }
            set
            {
                filter.Query = value;
            }
        }

        public override bool CanOk
        {
            get
            {
                return SelectedItems.Count() > 0;
            }
        }

        protected override void OnOk()
        {
            base.OnOk();
            onOk(SelectedItems);
            foreach (var item in SelectedItems)
            {
                item.IsSelected = false;
            }
        }

        public SelectorViewModel(IEnumerable<EntityBaseVM> entities, Action<IEnumerable<EntityBaseVM>> ok_handler)
        {
            onOk = ok_handler;
            filter = new FilterViewModel<EntityBaseVM>(entities, true);
            filter.UpdateResultsOnQueryChanges = true;
        }
    }
}