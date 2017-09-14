using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaskTracker.Model;
using TaskTracker.Repository;


namespace TaskTracker.ViewModels
{
    public class ProjectFilterItemViewModel : CheckableComboBoxItemViewModel
    {
        public ProjectFilterItemViewModel(Project s) : base(s.Name)
        { }
    }

    public class ProjectFilterViewModel : CheckableComboBoxViewModel<ProjectFilterItemViewModel>
    {
        public ProjectFilterViewModel(IEnumerable<ProjectFilterItemViewModel> collection) : base(collection)
        {
        }
    }
}
