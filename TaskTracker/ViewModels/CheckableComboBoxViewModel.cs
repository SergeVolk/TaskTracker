using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Utils;

namespace TaskTracker.ViewModels
{
    public class CheckableComboBoxItemViewModel : INotifyPropertyChanged
    {
        private bool isSelected = true;

        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public CheckableComboBoxItemViewModel(string name)
        {
            Title = name;
        }

        public string Title { get; set; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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
                    outString.Append(s.Title);
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
                    result.Add(s.Title);
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
