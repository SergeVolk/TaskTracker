using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using TaskTracker.Model;

namespace TaskTracker.Presentation.WPF.Converters
{
    internal abstract class OneWayValueConverterBase : IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #region TaskTracker Specific
    internal class StatusToVisibilityConverter : OneWayValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Status)value == Status.Closed ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    internal class ProgressOperationStatusToStringConverter : OneWayValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Status)value == Status.InProgress ? "Stop Progress" : "Start Progress";            
        }        
    }

    internal class CloseTaskStatusToStringConverter : OneWayValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Status)value == Status.Closed ? "Reopen Task" : "Close Task";            
        }        
    }

    internal class UserToStringConverter : OneWayValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((User)value).Name;
        }        
    }

    internal class ProjectToStringConverter : OneWayValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Project)value).Name;
        }        
    }

    internal class TaskTypeToStringConverter : OneWayValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TaskType)value).Name;
        }        
    }

    internal class StatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Status)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(Status), (String)value);
        }
    }

    internal class PriorityToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Priority)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(Priority), (String)value);
        }
    }

    internal class StageToTimeIntervalStringConverter : OneWayValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stage = (Stage)value;
            return $"{stage.StartTime} - {stage.EndTime}";
        }        
    }
    #endregion

    internal class ObjToEnableMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
                if (value == null)
                    return false;
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class BoolToVisibilityConverter : OneWayValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }        
    }

    internal class NullToVisibilityConverter : OneWayValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }        
    }

    // For debug purposes
    internal class ObjToObjConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debugger.Break();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debugger.Break();
            return value;
        }
    }
}
