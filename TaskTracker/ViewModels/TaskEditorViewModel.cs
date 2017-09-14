using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.ViewModels
{
    /*public interface ITaskEditViewModelProvider
    {
        TaskEditorViewModel GetViewModel(int taskId);
    }*/

    public class TaskEditorViewModel : INotifyPropertyChanged
    {
        private void Changed(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }

        public string Summary { get; set; }

        public string Description { get; set; }

        public string Estimation { get; set; }

        public string SelectedProject { get; set; }

        public string SelectedTaskType { get; set; }

        public string SelectedPriority { get; set; }

        public string SelectedAssignee { get; set; }

        public IEnumerable<string> Projects { get; set; }

        public IEnumerable<string> TaskTypes { get; set; }

        public IEnumerable<string> Priorities { get; set; }

        public IEnumerable<string> Assignees { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
