using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaskTracker.Model;
using TaskTracker.Repository;


namespace TaskTracker.ViewModels
{
    public class PriorityFilterItemViewModel : CheckableComboBoxItemViewModel
    {
        public PriorityFilterItemViewModel(Priority s) : base(s.ToString())
        { }
    }

    public class PriorityFilterViewModel : CheckableComboBoxViewModel<PriorityFilterItemViewModel>
    {
        public PriorityFilterViewModel(IEnumerable<PriorityFilterItemViewModel> collection) : base(collection)
        {
        }
    }
}
