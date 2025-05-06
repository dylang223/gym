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

            // Initialize the Workouts collection with sample data
            Workouts = new ObservableCollection<Workout>
            {
                // Push Workouts
                new Workout { Category = "Push", Exercise = "Bench Press", Reps = 8, Sets = 4, Weight = 50, Date = DateTime.Now.AddDays(-30) },
                new Workout { Category = "Push", Exercise = "Incline Dumbbell Press", Reps = 10, Sets = 3, Weight = 40, Date = DateTime.Now.AddDays(-25) },
                new Workout { Category = "Push", Exercise = "Overhead Shoulder Press", Reps = 10, Sets = 3, Weight = 30, Date = DateTime.Now.AddDays(-20) },
                new Workout { Category = "Push", Exercise = "Triceps Pushdown", Reps = 12, Sets = 3, Weight = 20, Date = DateTime.Now.AddDays(-15) },
                new Workout { Category = "Push", Exercise = "Lateral Raises", Reps = 15, Sets = 3, Weight = 10, Date = DateTime.Now.AddDays(-10) },

                // Pull Workouts
                new Workout { Category = "Pull", Exercise = "Deadlift", Reps = 6, Sets = 4, Weight = 80, Date = DateTime.Now.AddDays(-28) },
                new Workout { Category = "Pull", Exercise = "Pull-Ups", Reps = 10, Sets = 3, Weight = 0, Date = DateTime.Now.AddDays(-23) },
                new Workout { Category = "Pull", Exercise = "Barbell Rows", Reps = 8, Sets = 3, Weight = 60, Date = DateTime.Now.AddDays(-18) },
                new Workout { Category = "Pull", Exercise = "Face Pulls", Reps = 12, Sets = 3, Weight = 15, Date = DateTime.Now.AddDays(-13) },
                new Workout { Category = "Pull", Exercise = "Biceps Curls", Reps = 12, Sets = 3, Weight = 12, Date = DateTime.Now.AddDays(-8) },

                // Legs Workouts
                new Workout { Category = "Legs", Exercise = "Squats", Reps = 8, Sets = 4, Weight = 70, Date = DateTime.Now.AddDays(-27) },
                new Workout { Category = "Legs", Exercise = "Leg Press", Reps = 10, Sets = 3, Weight = 100, Date = DateTime.Now.AddDays(-22) },
                new Workout { Category = "Legs", Exercise = "Romanian Deadlifts", Reps = 10, Sets = 3, Weight = 50, Date = DateTime.Now.AddDays(-17) },
                new Workout { Category = "Legs", Exercise = "Walking Lunges", Reps = 12, Sets = 3, Weight = 20, Date = DateTime.Now.AddDays(-12) },
                new Workout { Category = "Legs", Exercise = "Calf Raises", Reps = 15, Sets = 3, Weight = 15, Date = DateTime.Now.AddDays(-7) }
            };

            // Bind the Workouts collection to the ListView
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