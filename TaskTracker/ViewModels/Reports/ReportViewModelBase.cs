using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskTracker.Repository;

namespace TaskTracker.ViewModels
{
    public abstract class ReportViewModelBase : ViewModelBase
    {
        protected IRepository Repository { get; private set; }

        protected ReportViewModelBase(IRepository repository)
        {
            this.Repository = repository;
            UpdateCommand = new Command<object>(OnUpdateCommand);
        }
        protected virtual void OnUpdateCommand(object sender)
        { }

        protected void Update()
        {
            if (UpdateCommand.CanExecute(null))
                UpdateCommand.Execute(null);
        }

        public ICommand UpdateCommand { get; private set; }
    }
}
