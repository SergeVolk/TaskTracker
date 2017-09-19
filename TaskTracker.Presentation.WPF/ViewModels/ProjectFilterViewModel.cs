using System;
using System.Collections.Generic;

using TaskTracker.ExceptionUtils;
using TaskTracker.Model;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class ProjectFilterItemViewModel : CheckableComboBoxItemViewModel
    {
        public ProjectFilterItemViewModel(Project project) : base(project.Name)
        {
            ArgumentValidation.ThrowIfNull(project, nameof(project));
        }
    }

    internal class ProjectFilterViewModel : CheckableComboBoxViewModel<ProjectFilterItemViewModel>
    {
        public ProjectFilterViewModel(IEnumerable<ProjectFilterItemViewModel> collection) : base(collection)
        { }
    }
}
