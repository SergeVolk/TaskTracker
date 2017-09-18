using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using TaskTracker.Client.WPF.Utils;
using TaskTracker.Common;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class CheckableComboBoxItemViewModel : SelectionItemViewModel
    {        
        public CheckableComboBoxItemViewModel(string name, bool isSelected = false)
        {
            ArgumentValidation.ThrowIfNullOrEmpty(name, nameof(name));

            this.Name = name;
            this.IsSelected = isSelected;
        }

        public string Name { get; private set; }        
    }

    public class CheckableComboBoxViewModel<T> : BindingList<T>
        where T : CheckableComboBoxItemViewModel
    {
        public static readonly string IsItemSelectedPropName = "IsSelected";

        public CheckableComboBoxViewModel(IEnumerable<T> collection) : base(collection.ToList())
        {
            ArgumentValidation.ThrowIfNull(collection, nameof(collection));
            ListChanged += OnItemsChanged;
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

        public void SetSelection(bool selState, IEnumerable<string> items)
        {
            ArgumentValidation.ThrowIfNull(items, nameof(items));

            this.Items.ForEach(item =>
            {                
                if (items.Contains(item.Name))
                    item.IsSelected = selState;
            });            
        }

        public event CheckableComboBoxItemSelectionChangeHandler ItemSelectionChanged;        

        private void OnItemsChanged(object sender, ListChangedEventArgs e)
        {
            if ((e.ListChangedType == ListChangedType.ItemChanged) && (e.PropertyDescriptor.Name == IsItemSelectedPropName))
            {
                var item = (sender as IList)?[e.NewIndex];
                if (item != null)
                    OnItemSelectionChanged(e.NewIndex, (bool)e.PropertyDescriptor.GetValue(item));
            }
        }

        private void OnItemSelectionChanged(int itemIndex, bool newSelectinoState)
        {
            var handler = ItemSelectionChanged;
            if (handler != null)
                handler(this, itemIndex, newSelectinoState);
        }        
    }

    public delegate void CheckableComboBoxItemSelectionChangeHandler(object sender, int itemIndex, bool newSelectinoState);
}
