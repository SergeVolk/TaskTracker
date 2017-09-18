using System;
using System.Collections.Generic;

using TaskTracker.Model;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class StatusFilterItemViewModel : CheckableComboBoxItemViewModel
    {
        public StatusFilterItemViewModel(Status s) : base(s.ToString())
        { }
    }

    public class StatusFilterViewModel : CheckableComboBoxViewModel<StatusFilterItemViewModel>
    {
        public StatusFilterViewModel(IEnumerable<StatusFilterItemViewModel> collection) : base(collection)
        { }
    }
}
