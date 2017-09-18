using System;
using System.Collections.Generic;
using System.ComponentModel;

using TaskTracker.Client.WPF.Utils;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
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
