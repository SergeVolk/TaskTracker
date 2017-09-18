using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Client.WPF.ViewModels;

namespace TaskTracker.Client.WPF.Views
{
    public class UIService : IUIService
    {
        bool? IUIService.ShowTaskCreationWindow(object dataContext)
        {
            var wnd = new TaskCreationWindow();
            wnd.DataContext = dataContext;
            return wnd.ShowDialog();
        }

        bool? IUIService.ShowTaskEditorWindow(object dataContext)
        {
            var editor = new TaskEditorWindow();
            editor.DataContext = dataContext;
            return editor.ShowDialog();
        }

        bool? IUIService.ShowInputDialog(string message, out string input)
        {
            var inputDialog = new InputBox(message);
            var result = inputDialog.ShowDialog();
            input = result.GetValueOrDefault() ? inputDialog.Input : "";
            return result;
        }
    }
}
