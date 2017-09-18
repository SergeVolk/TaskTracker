using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskTracker.Repository;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class TotalActivitiesTimeOfStageReportViewModel : ReportViewModelBase
    {
        private double totalStageTime;

        public TotalActivitiesTimeOfStageReportViewModel(IRepository repository) : base(repository)
        {}

        public int StageId { get; set; }

        public double TotalStageTime
        {
            get
            {
                return totalStageTime;
            }

            private set
            {
                if (totalStageTime != value)
                {
                    totalStageTime = value;
                    NotifyPropertyChanged(nameof(TotalStageTime));
                }
            }
        }        

        protected override void OnUpdateCommand(object sender)
        {            
            TotalStageTime = Repository.GetTotalActivityTimeOfStage(StageId);
        }
    }
}
