using System;
using System.Collections.Generic;
using System.Linq;

using TaskTracker.ExceptionUtils;
using TaskTracker.Model;
using TaskTracker.Repository;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class StageWithTaskCountVM
    {
        public StageWithTaskCountVM(Stage stage, int taskCount)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            ArgumentValidation.ThrowIfLess(taskCount, 0, nameof(taskCount));

            this.Stage = stage;
            this.TaskCount = taskCount;
        }

        public Stage Stage { get; private set; }
        public int TaskCount { get; private set; }
    }

    internal class MaxTasksStagesReportViewModel : ReportViewModelBase
    {
        private IEnumerable<StageWithTaskCountVM> stages;
        
        public MaxTasksStagesReportViewModel(IRepository repository) : base(repository)
        { }

        public IEnumerable<StageWithTaskCountVM> Stages
        {
            get
            {
                if (stages == null)
                    Update();

                return stages;
            }

            private set { SetProperty(ref stages, value, nameof(Stages)); }
        }        

        protected override void OnUpdateCommand(object sender)
        {
            var stagesWithTC = Repository.GetStagesWithMaxTasks(5);
            Stages = stagesWithTC.Select(e => new StageWithTaskCountVM(e.Item1, e.Item2));            
        }
    }
}
