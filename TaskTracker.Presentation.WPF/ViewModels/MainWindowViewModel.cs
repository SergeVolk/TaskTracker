using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

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
        private static readonly string DefaultAssignee = "Serge";

        private IRepository repository;
        private Status defaultStatus;
        private User defaultUser;
        private IUIService uiService;
        private TaskViewerViewModel selectedTask;
        private IEnumerable<TaskViewerViewModel> taskViewerViewModels;
        
        public MainWindowViewModel(IUIService uiService, IRepository repository)
        {
            ArgumentValidation.ThrowIfNull(uiService, nameof(uiService));
            ArgumentValidation.ThrowIfNull(repository, nameof(repository));

            this.uiService = uiService;
            this.repository = repository; 

            defaultStatus = Status.Open;
            defaultUser = repository.GetUsers().First(u => u.Name == DefaultReporter);

            ProjectFilterVM = new ProjectFilterViewModel(repository.GetProjects().Select(p => new ProjectFilterItemViewModel(p)));
            StatusFilterVM = new StatusFilterViewModel(EnumUtils.GetValues<Status>().Select(s => new StatusFilterItemViewModel(s)));
            PriorityFilterVM = new PriorityFilterViewModel(EnumUtils.GetValues<Priority>().Select(p => new PriorityFilterItemViewModel(p)));

            ProjectFilterVM.ItemSelectionChanged += OnFilterItemChanged;
            StatusFilterVM.ItemSelectionChanged += OnFilterItemChanged;
            PriorityFilterVM.ItemSelectionChanged += OnFilterItemChanged;

            ProjectFilterVM.SetSelection(true);

            var statusesToDisplay = new[] { Status.Open, Status.InProgress };
            StatusFilterVM.SetSelection(true, statusesToDisplay.Select(s => s.ToString()));

            var prioritiesToDisplay = new[] { Priority.High, Priority.Normal };
            PriorityFilterVM.SetSelection(true, prioritiesToDisplay.Select(p => p.ToString()));
                        
            CreateTaskCommand = new Command<object>(OnButtonCreateTaskClicked);
            ShowAllTasksCommand = new Command<object>(OnButtonAllTasksClicked);

            TaskStageEditorVM = new StageTasksEditorViewModel(repository);
            ReportsVM = new ReportsVM(repository);
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
            private set { SetProperty(ref selectedTask, value, nameof(SelectedTask)); }
        }

        private Double? SafeParseDouble(string str)
        {
            double tmp;
            return Double.TryParse(str, out tmp) ? tmp : (double?)null;
        }

        private void OnButtonCreateTaskClicked(object sender)
        {
            var taskCreationVM = new TaskEditorViewModel()
            {
                Assignees = repository.GetUsers().Select(u => u.Name),
                Projects = repository.GetProjects().Select(p => p.Name),
                Priorities = Enum.GetNames(typeof(Priority)),
                TaskTypes = repository.GetTaskTypes().Select(tt => tt.Name),
                SelectedAssignee = DefaultAssignee
            };
                        
            if (uiService.ShowTaskCreationWindow(taskCreationVM))
            {
                Priority priority;
                if (!Enum.TryParse(taskCreationVM.SelectedPriority, out priority))
                    priority = Priority.Normal;                

                double? estimation = SafeParseDouble(taskCreationVM.Estimation);
                                
                var task = new Task
                {
                    Summary = taskCreationVM.Summary,
                    Description = taskCreationVM.Description,
                    Priority = priority,
                    Creator = defaultUser,
                    Assignee = repository.GetUsers().First(u => u.Name == taskCreationVM.SelectedAssignee),
                    TaskTypeId = repository.GetTaskTypes().First(tt => tt.Name == taskCreationVM.SelectedTaskType).Id,
                    Project = repository.GetProjects().First(p => p.Name == taskCreationVM.SelectedProject),
                    Estimation = estimation,
                    Status = defaultStatus
                };

                repository.Add(task);
            }
        }

        private void OnButtonAllTasksClicked(object sender)
        {
            StatusFilterVM.SetSelection(true);
            ProjectFilterVM.SetSelection(true);
            PriorityFilterVM.SetSelection(true);

            QueryTasks();
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
                
            var tasks = repository.GetTasks(taskFilter, 
                new PropertySelector<Task>().
                    Select(t => t.Project).
                    Select(t => t.Assignee).
                    Select(t => t.Creator).
                    Select("Stage.Task"));            

            TaskViewerViewModels = tasks.Select(t => new TaskViewerViewModel(t, uiService, repository));
            SelectedTask = TaskViewerViewModels.FirstOrDefault();
        }

        private void OnFilterItemChanged(object sender, int itemIndex, bool newSelectinoState)
        {
            QueryTasks();
        }
    }
}
