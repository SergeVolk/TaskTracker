using System;
using System.Linq;

using TaskTracker.Repository;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class MaxActivitiesStageReportViewModel : ReportViewModelBase
    {
        private string stage;
        private int? activityCount;

        public MaxActivitiesStageReportViewModel(IRepository repository) : base(repository)
        { }

        public string Stage
        {
            get
            {
                if (String.IsNullOrEmpty(stage))
                    Update();
                
                return stage;
            }
            private set { SetProperty(ref stage, value, nameof(Stage)); }
        }

        public int ActivityCount
        {
            get
            {
                if (!activityCount.HasValue)
                    Update();

                return activityCount.GetValueOrDefault();
            }
            private set { SetProperty(ref activityCount, value, nameof(ActivityCount)); }
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
