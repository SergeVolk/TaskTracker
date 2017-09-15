using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Input;
using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.Utils;

namespace TaskTracker.ViewModels
{
    public class StageViewModel : ViewModelBase
    {
        private bool isSelected;
        private bool isExpanded;

        private StageViewModel parent;
        private Stage stage;
        private ObservableCollection<StageTaskViewModel> stageTasks;
        private StageTaskViewModel selectedStageTask;
        private IRepository repository;

        public StageViewModel(Stage stage, StageViewModel parent, Action<StageViewModel> selectionHandler, IRepository repository)
        {
            this.stage = stage;
            this.parent = parent;
            this.Selected += selectionHandler;
            this.repository = repository;

            this.SubStagesVM = new ReadOnlyCollection<StageViewModel>(stage.SubStages.Convert(s => new StageViewModel(s, this, selectionHandler, repository)));
            this.stageTasks = new ObservableCollection<StageTaskViewModel>(stage.Task.Convert(t => new StageTaskViewModel(t)));
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    NotifyPropertyChanged("IsSelected");

                    if (isSelected)
                        NotifySelected();
                }
            }
        }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (isExpanded && parent != null)
                    parent.IsExpanded = true;
            }
        }

        public string Name
        {
            get { return stage.Name; }
        }

        public void Remove(StageTaskViewModel taskVM)
        {
            stageTasks.Remove(taskVM);

            var ttMgr = new TaskTrackerManager(repository);
            ttMgr.RemoveTaskFromStage(taskVM.Task, stage);            
        }

        public void Add(StageTaskViewModel taskVM)
        {
            stageTasks.Add(taskVM);

            var ttMgr = new TaskTrackerManager(repository);
            ttMgr.AddTaskToStage(taskVM.Task, stage);
        }

        public IEnumerable<StageViewModel> SubStagesVM { get; private set; }

        public IEnumerable<StageTaskViewModel> StageTasks
        {
            get { return stageTasks; }            
        }

        public StageTaskViewModel SelectedStageTask
        {
            get { return selectedStageTask; }
            set
            {
                if (selectedStageTask != value)
                {
                    selectedStageTask = value;
                    NotifyPropertyChanged(nameof(SelectedStageTask));
                }
            }
        }

        public event Action<StageViewModel> Selected;

        private void NotifySelected()
        {
            var handler = Selected;
            if (handler != null)
                handler(this);
        }
    }

    public class StageTaskViewModel : ViewModelBase
    {
        public StageTaskViewModel(Task task)
        {
            this.Task = task;
            this.TaskPreviewLine = $"{Task.Project.ShortName}-{Task.Id}: {Task.Summary}";
        }

        public Task Task { get; private set; }

        public string TaskPreviewLine { get; private set; }        
    }

    public class StageTasksEditorViewModel : ViewModelBase
    {
        private IRepository repository;
        private StageViewModel selectedStageVM;
        private StageTaskViewModel selectedTask;
        private IEnumerable<StageViewModel> topLevelStagesVM;

        public StageTasksEditorViewModel(IRepository repository)
        {
            this.repository = repository;

            repository.GroupOperations(
               op =>
               {
                   var topLevelStages = op.GetStages(0);
                   topLevelStagesVM = new List<StageViewModel>(topLevelStages.Convert(s => new StageViewModel(s, null, OnStageSelected, repository)));

                   var tasks = op.GetTasks(t => t.Status != Status.Closed);
                   AllTasks = new List<StageTaskViewModel>(tasks.Convert(t => new StageTaskViewModel(t)));
               });

            topLevelStagesVM.ForEach(s => s.IsExpanded = true);

            RemoveTaskCommand = new Command<object>(OnRemoveTaskCommand);
            AddTaskCommand = new Command<object>(OnAddTaskCommand);
        }

        public IEnumerable<StageViewModel> TopLevelStagesVM
        {
            get { return topLevelStagesVM; }
            set
            {
                if (topLevelStagesVM != value)
                {
                    topLevelStagesVM = value;
                    NotifyPropertyChanged(nameof(TopLevelStagesVM));
                }
            }
        }

        public StageViewModel SelectedStageVM
        {
            get { return selectedStageVM; }
            set
            {
                if (selectedStageVM != value)
                {
                    if (selectedStageVM != null)
                        selectedStageVM.PropertyChanged -= OnPropertyChanged;

                    selectedStageVM = value;

                    if (selectedStageVM != null)
                        selectedStageVM.PropertyChanged += OnPropertyChanged;

                    NotifyPropertyChanged(nameof(SelectedStageVM));
                    NotifyPropertyChanged(nameof(SelectedStageTask));
                }
            }
        }

        public StageTaskViewModel SelectedTask
        {
            get { return selectedTask; }
            set
            {
                if (selectedTask != value)
                {
                    selectedTask = value;
                    NotifyPropertyChanged(nameof(SelectedTask));
                }
            }
        }

        public StageTaskViewModel SelectedStageTask
        {
            get { return SelectedStageVM?.SelectedStageTask; }            
        }

        public ICommand RemoveTaskCommand { get; private set; }
        public ICommand AddTaskCommand { get; private set; }

        public IEnumerable<StageTaskViewModel> AllTasks { get; private set; }

        private void OnRemoveTaskCommand(object sender)
        {
            var selTaskVM = SelectedStageVM?.SelectedStageTask;
            if (selTaskVM != null)
            {
                SelectedStageVM.Remove(selTaskVM);
                SelectedStageVM.SelectedStageTask = null;
            }
        }

        private void OnAddTaskCommand(object sender)
        {
            if (SelectedStageVM != null && SelectedTask != null)
                SelectedStageVM.Add(SelectedTask);
        }

        private void OnStageSelected(StageViewModel stage)
        {
            SelectedStageVM = stage;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedStageTask))
                NotifyPropertyChanged(nameof(SelectedStageTask));
        }
    }
}
