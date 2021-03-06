﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;

using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.Presentation.WPF.Utils;
using TaskTracker.Filters;
using TaskTracker.ExceptionUtils;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private static readonly string DefaultReporter = "Admin";
        private static readonly string DefaultAssignee = "User1";

        private IRepositoryQueries repositoryQueries;
        private ITransactionalRepositoryCommands repositoryCommands;
        private Status defaultStatus;
        private User defaultUser;
        private IUIService uiService;
        private TaskViewerViewModel selectedTask;
        private IEnumerable<TaskViewerViewModel> taskViewerViewModels;
        private UpdateManager queryTasksMgr;
        
        public MainWindowViewModel(IUIService uiService, IRepositoryQueries repositoryQueries, ITransactionalRepositoryCommands repositoryCommands)
        {
            ArgumentValidation.ThrowIfNull(uiService, nameof(uiService));
            ArgumentValidation.ThrowIfNull(repositoryQueries, nameof(MainWindowViewModel.repositoryQueries));
            ArgumentValidation.ThrowIfNull(repositoryCommands, nameof(MainWindowViewModel.repositoryCommands));

            this.uiService = uiService;
            this.repositoryQueries = repositoryQueries;
            this.repositoryCommands = repositoryCommands;

            if (!repositoryQueries.GetProjects().Any() || !repositoryQueries.GetTaskTypes().Any() || !repositoryQueries.GetUsers().Any())
            {
                uiService.ShowMessageBox(
                    @"The TaskTracker Repository has not been configured yet. Please configurate it using the tool 'DBManager'.", 
                    "Repository Error");
                throw new ApplicationException("Repository is not configured.");
            }

            queryTasksMgr = new UpdateManager(QueryTasks);

            defaultStatus = Status.Open;
            defaultUser = repositoryQueries.GetUsers().First(u => u.Name.Equals(DefaultReporter));

            ProjectFilterVM = new ProjectFilterViewModel(repositoryQueries.GetProjects().Select(p => new ProjectFilterItemViewModel(p)));
            StatusFilterVM = new StatusFilterViewModel(EnumUtils.GetValues<Status>().Select(s => new StatusFilterItemViewModel(s)));
            PriorityFilterVM = new PriorityFilterViewModel(EnumUtils.GetValues<Priority>().Select(p => new PriorityFilterItemViewModel(p)));

            ProjectFilterVM.ItemSelectionChanged += OnFilterItemChanged;
            StatusFilterVM.ItemSelectionChanged += OnFilterItemChanged;
            PriorityFilterVM.ItemSelectionChanged += OnFilterItemChanged;

            queryTasksMgr.BeginUpdate();
            try
            {
                ProjectFilterVM.SetSelection(true);

                var statusesToDisplay = new[] { Status.Open, Status.InProgress };
                StatusFilterVM.SetSelection(true, statusesToDisplay.Select(s => s.ToString()));

                var prioritiesToDisplay = new[] { Priority.High, Priority.Normal };
                PriorityFilterVM.SetSelection(true, prioritiesToDisplay.Select(p => p.ToString()));
            }
            finally
            {
                queryTasksMgr.EndUpdate();
            }
                        
            CreateTaskCommand = new Command<object>(OnButtonCreateTaskClicked);
            ShowAllTasksCommand = new Command<object>(OnButtonAllTasksClicked);

            TaskStageEditorVM = new StageTasksEditorViewModel(repositoryQueries, repositoryCommands);
            ReportsVM = new ReportsVM(repositoryQueries);
        }

        public ProjectFilterViewModel ProjectFilterVM { get; private set; }       

        public StatusFilterViewModel StatusFilterVM { get; private set; }

        public PriorityFilterViewModel PriorityFilterVM { get; private set; }

        public IEnumerable<TaskViewerViewModel> TaskViewerViewModels
        {
            get { return taskViewerViewModels; }
            private set { SetProperty(ref taskViewerViewModels, value, nameof(TaskViewerViewModels)); }
        }

        public StageTasksEditorViewModel TaskStageEditorVM { get; private set; } 
        
        public ReportsVM ReportsVM { get; private set; }

        public ICommand CreateTaskCommand { get; private set; }

        public ICommand ShowAllTasksCommand { get; private set; }
                
        public TaskViewerViewModel SelectedTask
        {
            get { return selectedTask; }
            set { SetProperty(ref selectedTask, value, nameof(SelectedTask)); }
        }

        private void OnButtonCreateTaskClicked(object sender)
        {
            var taskCreationVM = new TaskEditorViewModel()
            {
                Assignees = repositoryQueries.GetUsers().Select(u => u.Name),
                Projects = repositoryQueries.GetProjects().Select(p => p.Name),
                Priorities = Enum.GetNames(typeof(Priority)),
                TaskTypes = repositoryQueries.GetTaskTypes().Select(tt => tt.Name),
                SelectedAssignee = DefaultAssignee
            };
                        
            if (uiService.ShowTaskCreationWindow(taskCreationVM))
            {
                Debug.Assert(!String.IsNullOrEmpty(taskCreationVM.SelectedProject));
                Debug.Assert(!String.IsNullOrEmpty(taskCreationVM.SelectedTaskType));
                Debug.Assert(!String.IsNullOrEmpty(taskCreationVM.SelectedPriority));

                Priority priority = (Priority)Enum.Parse(typeof(Priority), taskCreationVM.SelectedPriority);

                double? estimation = ConversionUtils.SafeParseDouble(taskCreationVM.Estimation);
                string selAssignee = taskCreationVM.SelectedAssignee;

                var task = new Task
                {
                    Summary = taskCreationVM.Summary,
                    Description = taskCreationVM.Description,
                    Priority = priority,
                    Creator = defaultUser,
                    Assignee = !String.IsNullOrEmpty(selAssignee) ? repositoryQueries.GetUsers().First(u => u.Name.Equals(selAssignee)) : null,
                    TaskTypeId = repositoryQueries.GetTaskTypes().First(tt => tt.Name.Equals(taskCreationVM.SelectedTaskType)).Id,
                    Project = repositoryQueries.GetProjects().First(p => p.Name.Equals(taskCreationVM.SelectedProject)),
                    Estimation = estimation,
                    Status = defaultStatus
                };

                repositoryCommands.Add(task);
            }
        }

        private void OnButtonAllTasksClicked(object sender)
        {
            queryTasksMgr.BeginUpdate();
            try
            {
                StatusFilterVM.SetSelection(true);
                ProjectFilterVM.SetSelection(true);
                PriorityFilterVM.SetSelection(true);
            }
            finally
            {
                queryTasksMgr.EndUpdate();
            }            
        }

        private static int PriorityOrderer(Priority p)
        {
            switch (p)
            {
                case Priority.Low:
                    return 0;
                case Priority.Normal:
                    return 1;
                case Priority.High:
                    return 2;
                default:
                    throw ExceptionFactory.NotSupported(p);
            }
        }
                
        private void QueryTasks()
        {                
            var selectedStatuses = (StatusFilterVM != null) ? StatusFilterVM.GetSelectedItems() : Enumerable.Empty<string>();
            var selectedProjects = (ProjectFilterVM != null) ? ProjectFilterVM.GetSelectedItems() : Enumerable.Empty<string>();
            var selectedPriorities = (PriorityFilterVM != null) ? PriorityFilterVM.GetSelectedItems() : Enumerable.Empty<string>();

            var taskFilter = new TaskFilter();
            taskFilter.Statuses = selectedStatuses.ToList();
            taskFilter.Projects = selectedProjects.ToList();
            taskFilter.Priorities = selectedPriorities.ToList();
                
            var tasks = repositoryQueries.GetTasks(taskFilter, 
                new PropertySelector<Task>().
                    Select(t => t.Project).
                    Select(t => t.Assignee).
                    Select(t => t.Creator).
                    Select($"{nameof(Task.Stage)}.{nameof(Stage.Task)}"));                        

            TaskViewerViewModels = tasks.Select(t => new TaskViewerViewModel(t, uiService, repositoryQueries, repositoryCommands)).
                OrderByDescending(ks => ks.TaskId).
                OrderByDescending(ks => ks.Priority, Comparer<Priority>.Create((l, r) => PriorityOrderer(l).CompareTo(PriorityOrderer(r))));
            SelectedTask = TaskViewerViewModels.FirstOrDefault();
        }

        private void OnFilterItemChanged(object sender, int itemIndex, bool newSelectinoState)
        {
            queryTasksMgr.RequestUpdate();            
        }
    }
}
