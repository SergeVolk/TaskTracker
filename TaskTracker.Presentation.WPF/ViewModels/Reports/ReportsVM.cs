using System;
using System.Windows.Input;

using TaskTracker.ExceptionUtils;
using TaskTracker.Repository;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class ReportsVM : ViewModelBase
    {
        private IRepositoryQueries repositoryQueries;
        private CompositeCommand updateCommand;

        public ReportsVM(IRepositoryQueries repositoryQueries)
        {
            ArgumentValidation.ThrowIfNull(repositoryQueries, nameof(repositoryQueries));

            this.repositoryQueries = repositoryQueries;            

            MaxActivitiesStageReportVM = new MaxActivitiesStageReportViewModel(repositoryQueries);
            MaxTasksStagesReportVM = new MaxTasksStagesReportViewModel(repositoryQueries);
            TotalActivitiesTimeOfStageReportVM = new TotalActivitiesTimeOfStageReportViewModel(repositoryQueries);

            updateCommand = new CompositeCommand();
            updateCommand.Add(MaxActivitiesStageReportVM.UpdateCommand);
            updateCommand.Add(MaxTasksStagesReportVM.UpdateCommand);
            updateCommand.Add(TotalActivitiesTimeOfStageReportVM.UpdateCommand);
        }

        public ICommand UpdateCommand
        {
            get { return updateCommand; }
        }

        public MaxActivitiesStageReportViewModel MaxActivitiesStageReportVM { get; private set; }

        public MaxTasksStagesReportViewModel MaxTasksStagesReportVM { get; private set; }

        public TotalActivitiesTimeOfStageReportViewModel TotalActivitiesTimeOfStageReportVM { get; private set; }            
    }
}
