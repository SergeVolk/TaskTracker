using System;
using System.Windows.Input;

using TaskTracker.ExceptionUtils;
using TaskTracker.Repository;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal abstract class ReportViewModelBase : ViewModelBase
    {
        protected IRepositoryQueries RepositoryQueries { get; private set; }

        protected ReportViewModelBase(IRepositoryQueries repositoryQueries)
        {
            ArgumentValidation.ThrowIfNull(repositoryQueries, nameof(repositoryQueries));

            this.RepositoryQueries = repositoryQueries;
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
