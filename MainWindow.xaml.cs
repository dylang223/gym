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
using MongoDB.Bson;
using System.Threading.Tasks;

namespace gym
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Workout> Workouts { get; set; }
        private readonly WorkoutDB _workoutDB;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                // Initialize database
                _workoutDB = new WorkoutDB();

                // Initialize the Workouts collection with sample data
                Workouts = new ObservableCollection<Workout>
                {
                   
                };

                // Save sample data to database
                SaveSampleDataToDatabase();

                // Bind the Workouts collection to the ListView
                WorkoutListView.ItemsSource = Workouts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSampleDataToDatabase()
        {
            try
            {
                // Clear existing database entries
                var existingWorkouts = _workoutDB.GetAllWorkouts();
                if (existingWorkouts.Count == 0)
                {
                    foreach (var workout in Workouts)
                    {
                        _workoutDB.InsertWorkout(workout);
                    }
                    MessageBox.Show("Sample data saved to database successfully.", "Database",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving sample data: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenAddWorkoutWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the AddWorkoutWindow
                AddWorkoutWindow addWorkoutWindow = new AddWorkoutWindow();
                if (addWorkoutWindow.ShowDialog() == true && addWorkoutWindow.NewWorkout != null)
                {
                    // Make sure ID is set
                    if (string.IsNullOrEmpty(addWorkoutWindow.NewWorkout.Id))
                    {
                        addWorkoutWindow.NewWorkout.Id = ObjectId.GenerateNewId().ToString();
                    }

                    // Add the new workout to the collection
                    Workouts.Add(addWorkoutWindow.NewWorkout);

                    // Save to database
                    try
                    {
                        _workoutDB.InsertWorkout(addWorkoutWindow.NewWorkout);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving workout: {ex.Message}", "Database Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding workout: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenProgressWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                // Use database data directly instead of passing from here
                ProgressWindow progressWindow = new ProgressWindow();
                progressWindow.Owner = this; // Set owner to avoid window appearing behind
                progressWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening progress window: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
