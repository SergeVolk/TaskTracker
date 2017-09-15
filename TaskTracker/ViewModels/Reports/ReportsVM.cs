using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskTracker.Model;
using TaskTracker.Repository;

namespace TaskTracker.ViewModels
{
    public class ReportsVM : ViewModelBase
    {
        private IRepository repository;
        private CompositeCommand updateCommand;

        public ReportsVM(IRepository repository)
        {
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
