using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskTracker.Repository;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class MaxActivitiesStageReportViewModel : ReportViewModelBase
    {
        private string stage;
        private int? activityCount;

        public MaxActivitiesStageReportViewModel(IRepository repository) : base(repository)
        {
        }

        public string Stage
        {
            get
            {
                if (stage == null)
                    Update();
                
                return stage;
            }
            private set
            {
                if (stage != value)
                {
                    stage = value;
                    NotifyPropertyChanged(nameof(Stage));
                }
            }
        }

        public int ActivityCount
        {
            get
            {
                if (!activityCount.HasValue)
                    Update();

                return activityCount.GetValueOrDefault();
            }
            private set
            {
                if (activityCount != value)
                {
                    activityCount = value;
                    NotifyPropertyChanged(nameof(ActivityCount));
                }
            }
        }        

        protected override void OnUpdateCommand(object sender)
        {
            var maxActivityStageEntry = Repository.GetStagesWithMaxActivities(1).FirstOrDefault();
            var maxActivityStage = maxActivityStageEntry.Item1;
            Stage = $"{maxActivityStage.Name} [Level: {maxActivityStage.Level}, {maxActivityStage.StartTime}-{maxActivityStage.EndTime}]";
            ActivityCount = maxActivityStageEntry.Item2;
        }
    }
}
