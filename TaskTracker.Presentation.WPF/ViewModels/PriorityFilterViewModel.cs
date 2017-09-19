using System;
using System.Collections.Generic;

using TaskTracker.Model;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class PriorityFilterItemViewModel : CheckableComboBoxItemViewModel
    {
        public PriorityFilterItemViewModel(Priority s) : base(s.ToString())
        { }
    }

    internal class PriorityFilterViewModel : CheckableComboBoxViewModel<PriorityFilterItemViewModel>
    {
        public PriorityFilterViewModel(IEnumerable<PriorityFilterItemViewModel> collection) : base(collection)
        { }
    }
}
