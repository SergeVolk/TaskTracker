using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TaskTracker.Presentation.WPF.Views
{
    partial class CheckableComboBox : UserControl, INotifyPropertyChanged
    {
        public static readonly string IsItemSelectedPropName = "IsSelected";
        public static readonly string ItemNamePropTag = "Name";

        private DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(ItemsSourceProperty, typeof(CheckableComboBox));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IBindingList), typeof(CheckableComboBox), new UIPropertyMetadata(null));
        public static readonly DependencyProperty DefaultTextProperty = DependencyProperty.Register("DefaultText", typeof(string), typeof(CheckableComboBox), new UIPropertyMetadata(null));

        public CheckableComboBox()
        {
            InitializeComponent();

            descriptor.AddValueChanged(this, (sender, e) =>
            {
                var items = (IBindingList)this.GetValue(ItemsSourceProperty);
                items.ListChanged += OnItemsChanged;
            });
        }

        public IBindingList ItemsSource
        {
            get { return (IBindingList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); } 
        }

        public string Text
        {
            get { return GetItemsAsString(ItemsSource); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnItemsChanged(object sender, ListChangedEventArgs e)
        {
            if ((e.ListChangedType == ListChangedType.ItemChanged) && (e.PropertyDescriptor.Name == IsItemSelectedPropName))
            {
                NotifyPropertyChanged(nameof(Text));
            }
        }

        private void NotifyPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private string GetItemsAsString(IEnumerable items)
        {
            if (items == null)
                return DefaultText;

            StringBuilder outString = new StringBuilder();
            foreach (var item in items)
            {
                bool isSelected;
                if (!TryGetPropertyValue(item, IsItemSelectedPropName, out isSelected))
                    isSelected = false;

                string name;
                if (!TryGetPropertyValue(item, ItemNamePropTag, out name))
                    name = "";                

                if (isSelected)
                {
                    outString.Append(name);
                    outString.Append(',');
                }
            }
            var result = outString.ToString().TrimEnd(new char[] { ',' });
            return String.IsNullOrEmpty(result) ? DefaultText : result;
        }

        private static bool TryGetPropertyValue<T>(object obj, string propertyName, out T value)
        {
            value = default(T);
            var objType = obj.GetType();
            var prop = objType.GetProperty(propertyName);
            bool result = prop != null;
            if (result)
            {
                var valueObj = prop.GetValue(obj);
                if (valueObj != null)
                    value = (T)valueObj;                
            }
            return result;
        }
    }
}
