using System;

using TaskTracker.Client.WPF.ViewModels;

namespace TaskTracker.Client.WPF.Views
{
    public class UIService : IUIService
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

        private class InputDialogDataContext
        {
            public string Message { get; set; }
            public string Input { get; set; }
        }
    }
}
