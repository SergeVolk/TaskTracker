using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Client.WPF.Utils;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class CheckableComboBoxItemViewModel : ViewModelBase
    {
        private bool isSelected;
        
        public CheckableComboBoxItemViewModel(string name, bool isSelected = true)
        {
            this.Name = name;
            this.isSelected = isSelected;
        }

        public string Name { get; private set; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }        
    }

    public class CheckableComboBoxViewModel<T> : ObservableCollection<T>
        where T : CheckableComboBoxItemViewModel
    {
        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "IsSelected") && (sender is T))
            {
                var item = (T)sender;
                OnItemSelectionChanged(IndexOf(item), item.IsSelected);
            }
        }

        private void OnItemSelectionChanged(int itemIndex, bool newSelectinoState)
        {
            var handler = ItemSelectionChanged;
            if (handler != null)
                handler(this, itemIndex, newSelectinoState);
        }

        public CheckableComboBoxViewModel(IEnumerable<T> collection) : base(collection)
        {
            collection.ForEach(item => item.PropertyChanged += OnItemPropertyChanged);
        }

        public override string ToString()
        {
            StringBuilder outString = new StringBuilder();
            foreach (var s in this.Items)
            {
                if (s.IsSelected)
                {
                    outString.Append(s.Name);
                    outString.Append(',');
                }
            }
            return outString.ToString().TrimEnd(new char[] { ',' });
        }

        public IEnumerable<string> GetSelectedItems()
        {
            var result = new List<string>();
            foreach (var s in this.Items)
            {
                if (s.IsSelected)
                    result.Add(s.Name);
            }
            return result;
        }

        public void SetSelection(bool selState)
        {
            foreach (var s in this.Items)
            {
                s.IsSelected = selState;
            }
        }

        public event CheckableComboBoxItemSelectionChangeHandler ItemSelectionChanged;
    }

    public delegate void CheckableComboBoxItemSelectionChangeHandler(object sender, int itemIndex, bool newSelectinoState);
}
