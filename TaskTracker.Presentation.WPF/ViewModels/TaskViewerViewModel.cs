﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.ExceptionUtils;
using TaskTracker.Presentation.WPF.Utils;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class TaskViewerViewModel : ViewModelBase, IDescriptionProvider, IActivityProvider
    {
        private int taskId;
        private string shortTaskName;
        private string summary;
        private string description;
        private string estimation;
        private Status status;
        private Priority priority; 
        private string assignee;
        private string reporter;
        private string taskType;
        private string project;
        private IRepositoryQueries repositoryQueries;
        private ITransactionalRepositoryCommands repositoryCommands;
        private IUIService uiService;

        public TaskViewerViewModel(Task task, IUIService uiService, IRepositoryQueries repositoryQueries, ITransactionalRepositoryCommands repositoryCommands)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            ArgumentValidation.ThrowIfNull(uiService, nameof(uiService));
            ArgumentValidation.ThrowIfNull(repositoryQueries, nameof(repositoryQueries));
            ArgumentValidation.ThrowIfNull(repositoryCommands, nameof(repositoryCommands));

            this.uiService = uiService;
            this.repositoryQueries = repositoryQueries;
            this.repositoryCommands = repositoryCommands;

            EditTaskCommand = new Command<object>(OnButtonEditClicked);
            CloseTaskCommand = new Command<object>(OnButtonCloseTaskClicked);
            ChangeProgressCommand = new Command<object>(OnButtonProgressOperationClicked);

            Assign(task);
        }    

        public bool AllowAutogeneratedActivities { get; set; }

        public int TaskId
        {
            get { return taskId; }
            private set { SetProperty(ref taskId, value, nameof(TaskId), nameof(ShortTaskName)); }
        }

        public string ShortTaskName
        {
            get { return shortTaskName; }
            private set { SetProperty(ref shortTaskName, value, nameof(ShortTaskName)); }
        }

        public string Project
        {
            get { return project; }
            set { SetProperty(ref project, value, nameof(Project), nameof(ShortTaskName)); }
        }

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

        public Priority Priority
        {
            get { return priority; }
            set { SetProperty(ref priority, value, nameof(Priority)); }
        }

        public string Reporter
        {
            get { return reporter; }
            set { SetProperty(ref reporter, value, nameof(Reporter)); }
        }

        public string Assignee
        {
            get { return assignee; }
            set { SetProperty(ref assignee, value, nameof(Assignee)); }
        }

        public string TaskType
        {
            get { return taskType; }
            set { SetProperty(ref taskType, value, nameof(TaskType)); }
        }

        public string Estimation
        {
            get { return estimation; }
            set { SetProperty(ref estimation, value, nameof(Estimation)); }
        }

        public Status Status
        {
            get { return status; }
            set { SetProperty(ref status, value, nameof(Status)); }
        }

        public ICommand EditTaskCommand { get; private set; }

        public ICommand CloseTaskCommand { get; private set; }

        public ICommand ChangeProgressCommand { get; private set; }
        
        private void OnButtonEditClicked(object sender)
        {
            var taskEditorVM = new TaskEditorViewModel()
            {
                Assignees = repositoryQueries.GetUsers().Select(u => u.Name),
                Projects = repositoryQueries.GetProjects().Select(p => p.Name),
                Priorities = Enum.GetNames(typeof(Priority)),
                TaskTypes = repositoryQueries.GetTaskTypes().Select(tt => tt.Name),
                Description = Description,
                Estimation = Estimation,
                Summary = Summary,
                SelectedAssignee = Assignee,
                SelectedProject = Project,
                SelectedPriority = Priority.ToString(),
                SelectedTaskType = TaskType
            };

            if (uiService.ShowTaskEditorWindow(taskEditorVM))
            {
                Debug.Assert(!String.IsNullOrEmpty(taskEditorVM.SelectedProject));
                Debug.Assert(!String.IsNullOrEmpty(taskEditorVM.SelectedTaskType));
                Debug.Assert(!String.IsNullOrEmpty(taskEditorVM.SelectedPriority));

                double? estimation = ConversionUtils.SafeParseDouble(taskEditorVM.Estimation);                
                string selAssignee = taskEditorVM.SelectedAssignee;

                var task = new Task
                {
                    Id = TaskId,
                    Priority = (Priority)Enum.Parse(typeof(Priority), taskEditorVM.SelectedPriority),
                    Assignee = !String.IsNullOrEmpty(selAssignee) ? repositoryQueries.GetUsers().First(u => u.Name.Equals(selAssignee)) : null,
                    Creator = repositoryQueries.GetUsers().First(u => u.Name.Equals(Reporter)),
                    Project = repositoryQueries.GetProjects().First(p => p.Name.Equals(taskEditorVM.SelectedProject)),
                    TaskTypeId = repositoryQueries.GetTaskTypes().First(tt => tt.Name.Equals(taskEditorVM.SelectedTaskType)).Id,
                    Description = taskEditorVM.Description,
                    Estimation = estimation,
                    Summary = taskEditorVM.Summary
                };
                repositoryCommands.Update(task);
                Assign(task);
            }
        }

        private void OnButtonCloseTaskClicked(object sender)
        {
            switch (Status)
            {
                case Status.Open:
                case Status.InProgress:
                    SetTaskStatus(Status.Closed);
                    break;
                case Status.Closed:
                    SetTaskStatus(Status.Open);
                    break;
                default:
                    throw ExceptionFactory.NotSupported(Status);
            }
        }

        private void OnButtonProgressOperationClicked(object sender)
        {
            switch (Status)
            {
                case Status.Open:
                    SetTaskStatus(Status.InProgress);
                    break;
                case Status.InProgress:
                    SetTaskStatus(Status.Open);
                    break;
                default:
                    throw ExceptionFactory.NotSupported(Status);
            }
        }

        private void SetTaskStatus(Status newStatus)
        {
            Debug.Assert(Status != newStatus);
            
            var ttMgr = new TaskTrackerManager(repositoryQueries, repositoryCommands);
            switch (newStatus)
            {
                case Status.Open:
                {
                    if (Status == Status.InProgress)
                    {
                        ttMgr.StopTaskProgress(TaskId, this);
                    }
                    else
                    {
                        ttMgr.ReopenTask(TaskId);
                    }
                    break;
                }
                case Status.InProgress:
                    ttMgr.StartTaskProgress(TaskId);
                    break;
                case Status.Closed:
                    ttMgr.CloseTask(TaskId, this);
                    break;
                default:
                    throw ExceptionFactory.NotSupported(newStatus);
            }

            Status = newStatus;
        }

        private void Assign(Task task)
        {
            Debug.Assert(task != null);
            Debug.Assert(task.Id >= 0);
            Debug.Assert(task.Project != null);
            Debug.Assert(!String.IsNullOrEmpty(task.Project.ShortName));
            Debug.Assert(!String.IsNullOrEmpty(task.Summary));
            Debug.Assert(task.Creator != null);

            var taskType = repositoryQueries.FindTaskType(task.TaskTypeId);
            Debug.Assert(taskType != null);
            Debug.Assert(!String.IsNullOrEmpty(taskType.Name));

            this.TaskId = task.Id;
            this.ShortTaskName = $"{task.Project.ShortName}-{task.Id}";
            this.Project = task.Project.Name;
            this.Summary = task.Summary;
            this.Description = task.Description;
            this.Priority = task.Priority;
            this.Reporter = task.Creator.Name;
            this.Assignee = task.Assignee?.Name;
            this.TaskType = taskType.Name;
            this.Estimation = task.Estimation?.ToString();
            this.Status = task.Status;
        }

        string IDescriptionProvider.GetDescription(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));

            if (task.Status != Status.InProgress)
                throw ExceptionFactory.InvalidTaskStatus(task.Status, Status.InProgress, task.Id);

            string result;
            string msg = $"The work on the task '{task.Summary}' is being stopped.{Environment.NewLine}Input a comment.";
            if (!uiService.ShowInputDialog(msg, out result))
                result = "";

            return result;
        }

        Activity IActivityProvider.GetActivity(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));

            return AllowAutogeneratedActivities ?
                new Activity
                {
                    StartTime = DateTime.Now - TimeSpan.FromMinutes(10),
                    EndTime = DateTime.Now,
                    Task = task,
                    User = task.Assignee,
                    Description = $"Task {task.Summary} Autogenerated activity for small task"
                } :
                null;
        }
    }
}
