﻿using System;
using System.Linq;

using TaskTracker.Model;
using TaskTracker.Repository;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class MaxActivitiesStageReportViewModel : ReportViewModelBase
    {
        private Stage stage;
        private int? activityCount;

        public MaxActivitiesStageReportViewModel(IRepositoryQueries repositoryQueries) : base(repositoryQueries)
        { }

        public Stage Stage
        {
            get
            {
                if (stage == null)
                    Update();
                
                return stage;
            }
            private set { SetProperty(ref stage, value, nameof(Stage)); }
        }

        public string StageSummary
        {
            get
            {                
                return Stage != null ? $"{Stage.Name} [Level: {Stage.Level}, {Stage.StartTime}-{Stage.EndTime}]" : "<No stages>";
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
            private set { SetProperty(ref activityCount, value, nameof(ActivityCount)); }
        }        

        protected override void OnUpdateCommand(object sender)
        {
            var maxActivityStageEntry = RepositoryQueries.GetStagesWithMaxActivities(1).FirstOrDefault();
            if (maxActivityStageEntry != null)
            {
                Stage = maxActivityStageEntry.Item1;
                ActivityCount = maxActivityStageEntry.Item2;
            }
            else
            {
                Stage = null;
                ActivityCount = 0;
            }
        }
    }
}
