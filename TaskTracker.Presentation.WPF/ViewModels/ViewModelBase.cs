using System;
using System.Collections.Generic;
using System.ComponentModel;

using TaskTracker.Presentation.WPF.Utils;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class ViewModelBase : INotifyPropertyChanged
    {
        protected void NotifyPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProp>(ref TProp orgValue, TProp newValue, params string[] changedProperties)
        {
            if (!EqualityComparer<TProp>.Default.Equals(orgValue, newValue))
            {
                orgValue = newValue;
                changedProperties.ForEach(p => NotifyPropertyChanged(p));  
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
