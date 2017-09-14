using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TaskTracker.ViewModels
{
    public class Command<T> : ICommand
    {
        readonly Action<T> handler;
        readonly Predicate<T> canExecutePredicate;

        public Command(Action<T> handler) : this(handler, null)
        { }

        public Command(Action<T> handler, Predicate<T> canExecutePredicate)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            this.handler = handler;
            this.canExecutePredicate = canExecutePredicate;
        }

        public bool CanExecute(object parameter)
        {
            return canExecutePredicate == null ? true : canExecutePredicate((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            handler((T)parameter);
        }
    }
}
