using System;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal interface IUIService
    {
        bool ShowTaskCreationWindow(object dataContext);

        bool ShowTaskEditorWindow(object dataContext);

        bool ShowInputDialog(string message, out string input);
    }
}
