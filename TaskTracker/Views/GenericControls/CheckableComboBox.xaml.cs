using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskTracker.Views
{
    public partial class CheckableComboBox : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(CheckableComboBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(CheckableComboBox), new UIPropertyMetadata(string.Empty));        

        public CheckableComboBox()
        {
            InitializeComponent();            
        }

        public object ItemsSource 
        {
            get
            {
                return (object)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public string Text
        {
            get
            {
                string result = (this.ItemsSource != null) ? this.ItemsSource.ToString() : this.DefaultText;

                if (string.IsNullOrEmpty(result))
                {
                    result = this.DefaultText;
                }
                return result;
            }
        }

        public string DefaultText
        {
            get
            {
                return (string)GetValue(DefaultTextProperty);
            }
            set
            {
                SetValue(DefaultTextProperty, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            NotifyPropertyChanged("Text");
        }
    }
}