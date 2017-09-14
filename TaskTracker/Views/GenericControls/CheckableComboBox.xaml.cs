using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class CheckableComboBox : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(CheckableComboBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CheckableComboBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(CheckableComboBox), new UIPropertyMetadata(string.Empty));

        public CheckableComboBox()
        {
            InitializeComponent();

            SetText();                 
        }

        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
                SetText();
            }
        }

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public string DefaultText
        {
            get
            {
                return (string)GetValue(DefaultTextProperty);
            }

            set { SetValue(DefaultTextProperty, value); }
        }

        public event EventHandler ComboClosed;

        public event CheckableComboBoxSelectionHandler SelectionChanged;

        private void OnSelectionChanged(CheckBox sender)
        {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, new CheckableComboBoxSelectionEventArgs(sender, sender.IsChecked.GetValueOrDefault(false)));
        }        

        private void Popup_Closed(object sender, EventArgs e)
        {
            var onComboClosedEvent = ComboClosed;
            if (onComboClosedEvent != null)
                onComboClosedEvent(this, EventArgs.Empty);
        }
        
        private void CheckBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            SetText();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetText();
            OnSelectionChanged(e.Source as CheckBox);
        }

        private void SetText()
        {
            this.Text = (this.ItemsSource != null) ? this.ItemsSource.ToString() : this.DefaultText;

            if (string.IsNullOrEmpty(this.Text))
            {
                this.Text = this.DefaultText;
            }
        }
    }

    public class CheckableComboBoxSelectionEventArgs : EventArgs
    {
        public CheckableComboBoxSelectionEventArgs(CheckBox sender, bool newState)
        {
            this.Sender = sender;
            this.NewState = newState;

        }
        public CheckBox Sender { get; private set; }
        public bool NewState { get; private set; }
    }

    public delegate void CheckableComboBoxSelectionHandler(object sender, CheckableComboBoxSelectionEventArgs e);
}