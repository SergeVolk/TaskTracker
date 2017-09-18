using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.Client.WPF.Utils;
using TaskTracker.Common;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IRepository repository;
        private Status defaultStatus;
        private User defaultUser;
        private IUIService uiService;
        private TaskViewerViewModel selectedTask;
        private IEnumerable<TaskViewerViewModel> taskViewerViewModels;
        private ProjectFilterViewModel projectFilterVM;
        
        public MainWindowViewModel(IUIService uiService, IRepository repository)
        {
            this.uiService = uiService;
            this.repository = repository; 

            defaultStatus = Status.Open;
            defaultUser = repository.GetUsers().First(u => u.Name == "Admin");

            ProjectFilterVM = new ProjectFilterViewModel(repository.GetProjects().Select(p => new ProjectFilterItemViewModel(p)));
            StatusFilterVM = new StatusFilterViewModel(EnumUtils.GetValues<Status>().Select(s => new StatusFilterItemViewModel(s)));
            PriorityFilterVM = new PriorityFilterViewModel(EnumUtils.GetValues<Priority>().Select(p => new PriorityFilterItemViewModel(p)));

            ProjectFilterVM.ItemSelectionChanged += OnFilterItemChanged;
            StatusFilterVM.ItemSelectionChanged += OnFilterItemChanged;
            PriorityFilterVM.ItemSelectionChanged += OnFilterItemChanged;

            QueryTasks();

            CreateTaskCommand = new Command<object>(OnButtonCreateTaskClicked);

            ShowAllTasksTaskCommand = new Command<object>(OnButtonAllTasksClicked);

            TaskStageEditorVM = new StageTasksEditorViewModel(repository);
            ReportsVM = new ReportsVM(repository);
        }

        public void OnFilterItemChanged(object sender, int itemIndex, bool newSelectinoState)
        {
            QueryTasks();
        }

        public ProjectFilterViewModel ProjectFilterVM
        {
            get { return projectFilterVM; }
            set
            {
                if (projectFilterVM != value)
                {
                    projectFilterVM = value;
                    NotifyPropertyChanged("ProjectFilterVM");
                }
            }
        }
        public StatusFilterViewModel StatusFilterVM { get; private set; }
        public PriorityFilterViewModel PriorityFilterVM { get; private set; }

        public IEnumerable<TaskViewerViewModel> TaskViewerViewModels
        {
            get { return taskViewerViewModels; }
            set
            {
                if (taskViewerViewModels != value)
                {
                    taskViewerViewModels = value;
                    NotifyPropertyChanged("TaskViewerViewModels");
                }
            }
        }

        public StageTasksEditorViewModel TaskStageEditorVM { get; private set; } 
        
        public ReportsVM ReportsVM { get; private set; }

        public ICommand CreateTaskCommand { get; private set; }

        public ICommand ShowAllTasksTaskCommand { get; private set; }
                
        public TaskViewerViewModel SelectedTask
        {
            get { return selectedTask; }
            set
            {
                if (selectedTask != value)
                {
                    selectedTask = value;
                    QueryTasks();
                }
            }
        }

        private void OnButtonCreateTaskClicked(object sender)
        {
            var taskCreationVM = new TaskEditorViewModel()
            {
                Assignees = repository.GetUsers().Select(u => u.Name),
                Projects = repository.GetProjects().Select(p => p.Name),
                Priorities = Enum.GetNames(typeof(Priority)),
                TaskTypes = repository.GetTaskTypes().Select(tt => tt.Name),
                SelectedAssignee = "Serge"
            };
                        
            if (uiService.ShowTaskCreationWindow(taskCreationVM).GetValueOrDefault())
            {
                Priority priority;
                if (!Enum.TryParse(taskCreationVM.SelectedPriority, out priority))
                {
                    priority = Priority.Normal;
                }

                double tmp;
                double? estimation = null;
                if (Double.TryParse(taskCreationVM.Estimation, out tmp))
                    estimation = tmp;

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
            var selectedStatuses = (StatusFilterVM != null) ? StatusFilterVM.GetSelectedItems() : new string[] { };
            var selectedProjects = (ProjectFilterVM != null) ? ProjectFilterVM.GetSelectedItems() : new string[] { };
            var selectedPriorities = (PriorityFilterVM != null) ? PriorityFilterVM.GetSelectedItems() : new string[] { };         
                        
            var taskFilter = new TaskFilter();
            taskFilter.Statuses = new List<string>(selectedStatuses);
            taskFilter.Projects = new List<string>(selectedProjects);
            taskFilter.Priorities = new List<string>(selectedPriorities);
                
            var tasks = repository.GetTasks(taskFilter, 
                new PropertySelector<Task>().
                    Select(t => t.Project).
                    Select(t => t.Assignee).
                    Select(t => t.Creator).
                    Select("Stage.Task"));            

            TaskViewerViewModels = tasks.Select(t => new TaskViewerViewModel(t, uiService, repository));
        }   
    }
}
