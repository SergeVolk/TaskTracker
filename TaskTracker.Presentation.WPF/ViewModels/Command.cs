﻿using System;
using System.Collections.Generic;
using System.Windows.Input;

using TaskTracker.ExceptionUtils;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class Command<T> : ICommand
    {
        readonly Action<T> handler;
        readonly Predicate<T> canExecutePredicate;

        public Command(Action<T> handler) : this(handler, null)
        { }

        public Command(Action<T> handler, Predicate<T> canExecutePredicate)
        {
            ArgumentValidation.ThrowIfNull(handler, nameof(handler));
            
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

    internal class CompositeCommand : ICommand
    {
        private List<ICommand> childCommands;

        public CompositeCommand()
        {
            childCommands = new List<ICommand>();
        }

        public void Add(ICommand command)
        {
            ArgumentValidation.ThrowIfNull(command, nameof(command));            
            childCommands.Add(command);
        }

        public bool CanExecute(object parameter)
        {
            foreach (var command in childCommands)
            {
                if (!command.CanExecute(parameter))
                {
                    return false;
                }
            }
            return true;
        }
        
        public void Execute(object parameter)
        {
            foreach (var command in childCommands)
            {
                command.Execute(parameter);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
