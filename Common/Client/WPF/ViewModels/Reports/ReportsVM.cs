using System;
using System.Windows.Input;

using TaskTracker.Common;
using TaskTracker.Repository;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class ReportsVM : ViewModelBase
    {
        private IRepository repository;
        private CompositeCommand updateCommand;

        public ReportsVM(IRepository repository)
        {
            ArgumentValidation.ThrowIfNull(repository, nameof(repository));

            this.repository = repository;            

            MaxActivitiesStageReportVM = new MaxActivitiesStageReportViewModel(repository);
            MaxTasksStagesReportVM = new MaxTasksStagesReportViewModel(repository);
            TotalActivitiesTimeOfStageReportVM = new TotalActivitiesTimeOfStageReportViewModel(repository);

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
