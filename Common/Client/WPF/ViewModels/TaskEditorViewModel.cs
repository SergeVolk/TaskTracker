using System;
using System.Collections.Generic;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class TaskEditorViewModel : ViewModelBase
    {
        private string summary;
        private string description;
        private string estimation;
        private string selectedProject;
        private string selectedTaskType;
        private string selectedPriority;
        private string selectedAssignee;
        private IEnumerable<string> projects;
        private IEnumerable<string> taskTypes;
        private IEnumerable<string> priorities;
        private IEnumerable<string> assignees;

        public string Summary
        {
            get { return summary; }
            set { SetProperty(ref summary, value, nameof(Summary)); }
        }

        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value, nameof(Description)); }
        }

        public string Estimation
        {
            get { return estimation; }
            set { SetProperty(ref estimation, value, nameof(Estimation)); }
        }

        public string SelectedProject
        {
            get { return selectedProject; }
            set { SetProperty(ref selectedProject, value, nameof(SelectedProject)); }
        }

        public string SelectedTaskType
        {
            get { return selectedTaskType; }
            set { SetProperty(ref selectedTaskType, value, nameof(SelectedTaskType)); }
        }

        public string SelectedPriority
        {
            get { return selectedPriority; }
            set { SetProperty(ref selectedPriority, value, nameof(SelectedPriority)); }
        }

        public string SelectedAssignee
        {
            get { return selectedAssignee; }
            set { SetProperty(ref selectedAssignee, value, nameof(SelectedAssignee)); }
        }

        public IEnumerable<string> Projects
        {
            get { return projects; }
            set { SetProperty(ref projects, value, nameof(Projects)); }
        }

        public IEnumerable<string> TaskTypes
        {
            get { return taskTypes; }
            set { SetProperty(ref taskTypes, value, nameof(TaskTypes)); }
        }

        public IEnumerable<string> Priorities
        {
            get { return priorities; }
            set { SetProperty(ref priorities, value, nameof(Priorities)); }
        }

        public IEnumerable<string> Assignees
        {
            get { return assignees; }
            set { SetProperty(ref assignees, value, nameof(Assignees)); }
        }
    }
}
