using System;
using System.Collections.Generic;

using TaskTracker.Model;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class StatusFilterItemViewModel : CheckableComboBoxItemViewModel
    {
        public StatusFilterItemViewModel(Status s) : base(s.ToString())
        { }
    }

    internal class StatusFilterViewModel : CheckableComboBoxViewModel<StatusFilterItemViewModel>
    {
        public StatusFilterViewModel(IEnumerable<StatusFilterItemViewModel> collection) : base(collection)
        { }
    }
}
