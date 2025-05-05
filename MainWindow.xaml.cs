using System.Collections.ObjectModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gym
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Workout> Workouts { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Workouts = new ObservableCollection<Workout>();
            WorkoutListView.ItemsSource = Workouts;
        }

        private void OpenAddWorkoutWindow(object sender, RoutedEventArgs e)
        {
            // Open the AddWorkoutWindow
            AddWorkoutWindow addWorkoutWindow = new AddWorkoutWindow();
            if (addWorkoutWindow.ShowDialog() == true)
            {
                // Add the new workout to the collection
                Workouts.Add(addWorkoutWindow.NewWorkout);
            }
        }

        private void OpenProgressWindow(object sender, RoutedEventArgs e)
        {
            if (Workouts.Count == 0)
            {
                MessageBox.Show("No workout data available to display progress.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Open the ProgressWindow and pass the workouts
            ProgressWindow progressWindow = new ProgressWindow(Workouts.ToList());
            progressWindow.ShowDialog();
        }
    }
}