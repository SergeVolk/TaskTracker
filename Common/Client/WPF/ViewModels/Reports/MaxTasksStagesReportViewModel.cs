using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskTracker.Model;
using TaskTracker.Repository;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class StageWithTaskCountVM
    {
        public StageWithTaskCountVM(Stage stage, int taskCount)
        {
            this.Stage = stage;
            this.TaskCount = taskCount;
        }

        public Stage Stage { get; private set; }
        public int TaskCount { get; private set; }
    }

    public class MaxTasksStagesReportViewModel : ReportViewModelBase
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

            private set
            {
                if (stages != value)
                {
                    stages = value;
                    NotifyPropertyChanged(nameof(Stages));
                }
            }
        }        

        protected override void OnUpdateCommand(object sender)
        {
            var stagesWithTC = Repository.GetStagesWithMaxTasks(5);
            Stages = stagesWithTC.Select(e => new StageWithTaskCountVM(e.Item1, e.Item2));            
        }
    }
}
