using System;
using System.Collections.Generic;

using TaskTracker.Common;
using TaskTracker.Model;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class ProjectFilterItemViewModel : CheckableComboBoxItemViewModel
    {
        public ProjectFilterItemViewModel(Project project) : base(project.Name)
        {
            ArgumentValidation.ThrowIfNull(project, nameof(project));
        }
    }

    public class ProjectFilterViewModel : CheckableComboBoxViewModel<ProjectFilterItemViewModel>
    {
        public ProjectFilterViewModel(IEnumerable<ProjectFilterItemViewModel> collection) : base(collection)
        { }
    }
}
