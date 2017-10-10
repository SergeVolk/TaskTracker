using System;
using System.Windows;

using TaskTracker.Presentation.WPF.ViewModels;
using TaskTracker.ExceptionUtils;

namespace TaskTracker.Presentation.WPF.Views
{
    internal class UIService : IUIService
    {
        bool IUIService.ShowTaskCreationWindow(object dataContext)
        {
            var wnd = new TaskCreationWindow();
            wnd.DataContext = dataContext;
            return wnd.ShowDialog().GetValueOrDefault();
        }

        bool IUIService.ShowTaskEditorWindow(object dataContext)
        {
            var editor = new TaskEditorWindow();
            editor.DataContext = dataContext;
            return editor.ShowDialog().GetValueOrDefault();
        }

        bool IUIService.ShowInputDialog(string message, out string input)
        {
            ArgumentValidation.ThrowIfNullOrEmpty(message, nameof(message));

            var dc = new InputDialogDataContext()
            {
                Message = message,
                Input = ""                
            };

            var inputDialog = new InputBox();
            inputDialog.DataContext = dc;
            var result = inputDialog.ShowDialog().GetValueOrDefault();
            input = dc.Input; 
            return result;
        }

        bool IUIService.ShowMessageBox(string message, string caption)
        {
            return MessageBox.Show(message, caption, MessageBoxButton.OK) == MessageBoxResult.OK;
        }

        private class InputDialogDataContext
        {
            public string Message { get; set; }
            public string Input { get; set; }
        }
    }
}
