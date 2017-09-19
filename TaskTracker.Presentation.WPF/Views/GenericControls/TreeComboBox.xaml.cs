using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace TaskTracker.Presentation.WPF.Views
{
    partial class TreeComboBox : UserControl
    {        
        public TreeComboBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemSelectedCommandProperty = DependencyProperty.Register("ItemSelectedCommand", typeof(ICommand), 
            typeof(TreeComboBox), new UIPropertyMetadata(null));
        
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable),
            typeof(TreeComboBox), new UIPropertyMetadata(null));
        
        public ICommand ItemSelectedCommand
        {
            get
            {
                return (ICommand)GetValue(ItemSelectedCommandProperty);
            }
            set
            {
                SetValue(ItemSelectedCommandProperty, value);
            }
        }

        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }        

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new NoArgsDelegate(() => 
            {
                comboBox.IsDropDownOpen = false;
            }));            

            var itemSelected = ItemSelectedCommand;
            if (itemSelected != null && itemSelected.CanExecute(e.NewValue))
                itemSelected.Execute(e.NewValue);
                        
            e.Handled = true;
        }

        private delegate void NoArgsDelegate();
    }
}
