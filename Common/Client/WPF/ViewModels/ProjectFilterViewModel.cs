using System;
using System.Collections.Generic;

using TaskTracker.Model;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class ProjectFilterItemViewModel : CheckableComboBoxItemViewModel
    {
        public ProjectFilterItemViewModel(Project s) : base(s.Name)
        { }
    }

    public class ProjectFilterViewModel : CheckableComboBoxViewModel<ProjectFilterItemViewModel>
    {
        public ProjectFilterViewModel(IEnumerable<ProjectFilterItemViewModel> collection) : base(collection)
        { }
    }
}
