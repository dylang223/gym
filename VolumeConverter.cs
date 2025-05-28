using System;
using System.Globalization;
using System.Windows.Data;

using System.Windows; // Add this namespace

namespace gym
{
    public class VolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int sets)
            {
                // Try to get the Workout directly
                if (parameter is Workout workoutParam)
                {
                    return (sets * workoutParam.Reps * workoutParam.Weight).ToString("F0");
                }

                // Direct calculation from bound object
                var fe = parameter as FrameworkElement; // FrameworkElement is defined in System.Windows
                if (fe != null)
                {
                    var workoutDataContext = fe.DataContext as Workout;
                    if (workoutDataContext != null)
                    {
                        return (sets * workoutDataContext.Reps * workoutDataContext.Weight).ToString("F0");
                    }
                }
            }
            return "0";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
