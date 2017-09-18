using System;

namespace TaskTracker.Client.WPF.ViewModels
{
    public interface IUIService
    {
        bool ShowTaskCreationWindow(object dataContext);

        bool ShowTaskEditorWindow(object dataContext);

        bool ShowInputDialog(string message, out string input);
    }
}
