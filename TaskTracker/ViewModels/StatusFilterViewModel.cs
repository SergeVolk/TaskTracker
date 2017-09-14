using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaskTracker.Model;
using TaskTracker.Repository;


namespace TaskTracker.ViewModels
{
    public class StatusFilterItemViewModel : CheckableComboBoxItemViewModel
    {
        public StatusFilterItemViewModel(Status s) : base(s.ToString())
        { }
    }

    public class StatusFilterViewModel : CheckableComboBoxViewModel<StatusFilterItemViewModel>
    {
        public StatusFilterViewModel(IEnumerable<StatusFilterItemViewModel> collection) : base(collection)
        {
        }
    }
}
