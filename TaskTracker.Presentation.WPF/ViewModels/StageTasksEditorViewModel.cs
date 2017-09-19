using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;

using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.Filters;
using TaskTracker.ExceptionUtils;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class StageViewModel : SelectionItemViewModel
    {
        private bool isExpanded;
        private StageViewModel parent;
        private Stage stage;
        private ObservableCollection<StageTaskViewModel> stageTasks;
        private StageTaskViewModel selectedStageTask;
        private IRepository repository;

        public StageViewModel(Stage stage, StageViewModel parent, Action<StageViewModel> selectionHandler, IRepository repository, bool isExpanded = false)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            ArgumentValidation.ThrowIfNull(selectionHandler, nameof(selectionHandler));
            ArgumentValidation.ThrowIfNull(repository, nameof(repository));

            this.stage = stage;
            this.parent = parent;            
            this.repository = repository;
            this.isExpanded = isExpanded;
            this.Selected += selectionHandler;
            this.SubStagesVM = stage.SubStages.Select(s => new StageViewModel(s, this, selectionHandler, repository)).ToList();
            this.stageTasks = new ObservableCollection<StageTaskViewModel>(stage.Task.Select(t => new StageTaskViewModel(t)).ToList());
        }        

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    NotifyPropertyChanged(nameof(IsExpanded));
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
            ArgumentValidation.ThrowIfNull(taskVM, nameof(taskVM));            

            stageTasks.Remove(taskVM);

            var ttMgr = new TaskTrackerManager(repository);
            ttMgr.RemoveTaskFromStage(taskVM.Task, stage);            
        }

        public void Add(StageTaskViewModel taskVM)
        {
            ArgumentValidation.ThrowIfNull(taskVM, nameof(taskVM));

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
            set { SetProperty(ref selectedStageTask, value, nameof(SelectedStageTask)); }
        }

        public event Action<StageViewModel> Selected;

        protected override void AfterSelectedChanged(bool newIsSelected)
        {
            base.AfterSelectedChanged(newIsSelected);

            if (newIsSelected)
                NotifySelected();
        }

        private void NotifySelected()
        {
            var handler = Selected;
            if (handler != null)
                handler(this);
        }
    }

    internal class StageTaskViewModel : ViewModelBase
    {
        public StageTaskViewModel(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));

            this.Task = task;
        }

        public Task Task { get; private set; }

        public string TaskPreviewLine
        {
            get { return $"{Task.Project.ShortName}-{Task.Id}: {Task.Summary}"; }
        }        
    }

    internal class StageTasksEditorViewModel : ViewModelBase
    {
        private IRepository repository;
        private StageViewModel selectedStageVM;
        private StageTaskViewModel selectedTask;
        private IEnumerable<StageViewModel> topLevelStagesVM;

        public StageTasksEditorViewModel(IRepository repository)
        {
            ArgumentValidation.ThrowIfNull(repository, nameof(repository));

            this.repository = repository;

            IEnumerable<Stage> topLevelStages = null;
            IEnumerable<Task> tasks = null;

            repository.GroupOperations(op =>
            {
                var stagePropsSelector = new PropertySelector<Stage>().Select("Task.Stage").Select("Task.Project");
                topLevelStages = op.GetStages(0, stagePropsSelector, true);                

                var taskFilter = new TaskFilter();
                taskFilter.Statuses = new List<string> { "Open", "InProgress" };

                tasks = op.GetTasks(taskFilter, new PropertySelector<Task>().
                    Select(t => t.Project).
                    Select(t => t.Assignee).
                    Select("Stage.Task"));                
            });

            TopLevelStagesVM = topLevelStages.Select(s => new StageViewModel(s, null, OnStageSelected, repository, true)).ToList();
            AllTasks = tasks.Select(t => new StageTaskViewModel(t)).ToList();

            RemoveTaskCommand = new Command<object>(OnRemoveTaskCommand);
            AddTaskCommand = new Command<object>(OnAddTaskCommand);
        }

        public IEnumerable<StageViewModel> TopLevelStagesVM
        {
            get { return topLevelStagesVM; }
            private set { SetProperty(ref topLevelStagesVM, value, nameof(TopLevelStagesVM)); }
        }

        public StageViewModel SelectedStageVM
        {
            get { return selectedStageVM; }
            private set
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
            private set { SetProperty(ref selectedTask, value, nameof(SelectedTask)); }
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
            Debug.Assert(SelectedStageVM != null, "No stage selected.");

            var selTaskVM = SelectedStageVM.SelectedStageTask;
            Debug.Assert(selTaskVM != null, "No tasks selected in the stage.");            
            
            SelectedStageVM.Remove(selTaskVM);
            SelectedStageVM.SelectedStageTask = null;            
        }

        private void OnAddTaskCommand(object sender)
        {
            Debug.Assert(SelectedStageVM != null, "No stage selected.");
            Debug.Assert(SelectedTask != null, "No task selected.");

            SelectedStageVM.Add(SelectedTask);
        }

        private void OnStageSelected(StageViewModel stage)
        {
            SelectedStageVM = stage;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(SelectedStageTask)))
                NotifyPropertyChanged(nameof(SelectedStageTask));
        }
    }
}
