using System;
using System.Windows.Input;

using TaskTracker.ExceptionUtils;
using TaskTracker.Repository;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal abstract class ReportViewModelBase : ViewModelBase
    {
        protected IRepository Repository { get; private set; }

        protected ReportViewModelBase(IRepository repository)
        {
            ArgumentValidation.ThrowIfNull(repository, nameof(repository));

            this.Repository = repository;
            UpdateCommand = new Command<object>(OnUpdateCommand);
        }

        protected virtual void OnUpdateCommand(object sender)
        {
            // empty
        }

        protected void Update()
        {
            if (UpdateCommand.CanExecute(null))
                UpdateCommand.Execute(null);
        }

        public ICommand UpdateCommand { get; private set; }
    }
}
