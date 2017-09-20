using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class TotalActivitiesTimeOfStageReportViewModel : ReportViewModelBase
    {
        private double totalStageTime;
        private int? selectedStageId;

        public TotalActivitiesTimeOfStageReportViewModel(IRepository repository) : base(repository)
        {
            StageSelectedCommand = new Command<object>(OnStageSelected);
        }
                
        public double TotalStageTime
        {
            get
            {
                return totalStageTime;
            }
            private set { SetProperty(ref totalStageTime, value, nameof(TotalStageTime)); }
        }
        
        public IEnumerable<StageTreeViewModel> TopLevelStages
        {
            get
            {
                var stages = Repository.GetStages(0, new PropertySelector<Stage>().Select(s => s.Task), true);
                return stages.Select(s => new StageTreeViewModel(s, true));
            }
        }

        public ICommand StageSelectedCommand { get; private set; }

        protected override void OnUpdateCommand(object sender)
        {
            SetSelectedStage(selectedStageId);
        }

        private void OnStageSelected(object sender)
        {
            int? stageId = null;
            if (sender != null)
            {
                if (sender is StageTreeViewModel)
                {
                    stageId = ((StageTreeViewModel)sender).Stage.Id;
                }
                else if (sender is Int32)
                {
                    stageId = (int)sender;
                }
            }

            SetSelectedStage(stageId);
        }

        private void SetSelectedStage(int? stageId)
        {
            if (selectedStageId != stageId)
            {
                selectedStageId = stageId;
                TotalStageTime = selectedStageId.HasValue ? Repository.GetTotalActivityTimeOfStage(selectedStageId.Value) : 0;
            }
        }
    }
}
