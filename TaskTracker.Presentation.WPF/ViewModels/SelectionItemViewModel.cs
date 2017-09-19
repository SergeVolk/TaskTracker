using System;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class SelectionItemViewModel : ViewModelBase 
    {
        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set 
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    NotifyPropertyChanged(nameof(IsSelected));

                    AfterSelectedChanged(isSelected);
                }
            }
        }

        protected virtual void AfterSelectedChanged(bool newIsSelected)
        {
            // empty
        }
    }
}
