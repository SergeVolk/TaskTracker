using System;
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
        private StageTreeViewModel selectedStage;

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
        
        public object TopLevelStages
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
            SetSelectedStage(selectedStage);
        }

        private void OnStageSelected(object sender)
        {
            var stageVM = sender as StageTreeViewModel;
            if (stageVM != null)
                SetSelectedStage(stageVM);
        }

        private void SetSelectedStage(StageTreeViewModel stage)
        {
            if (selectedStage != stage)
            {
                selectedStage = stage;
                TotalStageTime = selectedStage != null ? Repository.GetTotalActivityTimeOfStage(selectedStage.Stage.Id) : 0;
            }
        }
    }
}
