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
using System.Linq;

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

                // Set DataContext to this window for bindings to work
                DataContext = this;

                // Initialize database
                _workoutDB = new WorkoutDB();

                // Initialize an empty collection first
                Workouts = new ObservableCollection<Workout>();

                // Set the ItemsSource immediately with empty collection
                WorkoutListView.ItemsSource = Workouts;

                // Show the Workouts tab by default
                ShowTab("WorkoutsTab");

                // Load data asynchronously
                LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddSampleDataToDatabase()
        {
            try
            {
                var sampleWorkouts = CreateSampleWorkouts();
                foreach (var workout in sampleWorkouts)
                {
                    await _workoutDB.InsertWorkoutAsync(workout);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding sample data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void LoadDataAsync()
        {
            try
            {
                // First check if database has any data
                var existingWorkouts = await _workoutDB.GetAllWorkoutsAsync();

                // If database is empty, add sample data
                if (existingWorkouts == null || existingWorkouts.Count == 0)
                {
                    await AddSampleDataToDatabase();
                    // Retrieve the newly added data
                    existingWorkouts = await _workoutDB.GetAllWorkoutsAsync();
                }

                // Update the UI with the data from database
                await Dispatcher.InvokeAsync(() =>
                {
                    Workouts.Clear();

                    // Sort by date (newest first)
                    var sortedWorkouts = existingWorkouts.OrderByDescending(w => w.Date).ToList();

                    foreach (var workout in sortedWorkouts)
                    {
                        Workouts.Add(workout);
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        // Shows the selected tab and hides others
        private void ShowTab(string tabName)
        {
            foreach (TabItem tab in MainTabControl.Items)
            {
                if (tab.Name == tabName)
                {
                    tab.Visibility = Visibility.Visible;
                    tab.IsSelected = true;
                }
                else
                {
                    tab.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tabName)
            {
                ShowTab(tabName);
            }
        }

        private List<Workout> CreateSampleWorkouts()
        {
            var workouts = new List<Workout>();

            // Create dates that are evenly spaced for better line visualization
            var dates = new List<DateTime>();
            for (int i = 30; i >= 0; i -= 5) // Every 5 days for past month
            {
                dates.Add(DateTime.Now.AddDays(-i));
            }

            // Bench Press progression - smooth progression
            double benchWeight = 50;
            int benchReps = 8;
            foreach (var date in dates)
            {
                workouts.Add(new Workout
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Category = "Push",
                    Exercise = "Bench Press",
                    Reps = benchReps,
                    Sets = 4,
                    Weight = benchWeight,
                    Date = date
                });
                benchWeight += 2.5;
                if (benchReps < 12) benchReps++;
            }

            // Overhead Press progression
            double ohpWeight = 30;
            int ohpReps = 8;
            foreach (var date in dates)
            {
                workouts.Add(new Workout
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Category = "Push",
                    Exercise = "Overhead Shoulder Press",
                    Reps = ohpReps,
                    Sets = 3,
                    Weight = ohpWeight,
                    Date = date
                });
                ohpWeight += 1.25;
                if (ohpReps < 12) ohpReps++;
            }

            // Add remaining exercises with consistent progression
            // Triceps
            double tricepsWeight = 20;
            foreach (var date in dates)
            {
                workouts.Add(new Workout
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Category = "Push",
                    Exercise = "Triceps Pushdown",
                    Reps = 12,
                    Sets = 3,
                    Weight = tricepsWeight,
                    Date = date
                });
                tricepsWeight += 1;
            }

            // Deadlift
            double deadliftWeight = 80;
            foreach (var date in dates)
            {
                workouts.Add(new Workout
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Category = "Pull",
                    Exercise = "Deadlift",
                    Reps = 6,
                    Sets = 4,
                    Weight = deadliftWeight,
                    Date = date
                });
                deadliftWeight += 2.5;
            }

            // Squats
            double squatWeight = 70;
            foreach (var date in dates)
            {
                workouts.Add(new Workout
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Category = "Legs",
                    Exercise = "Squats",
                    Reps = 8,
                    Sets = 4,
                    Weight = squatWeight,
                    Date = date
                });
                squatWeight += 2.5;
            }

            return workouts;
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
        // New method for handling template workouts
        private void LogTemplateWorkout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string exerciseName)
                {
                    // Pre-fill exercise name in the add workout window
                    AddWorkoutWindow addWorkoutWindow = new AddWorkoutWindow();

                    // Determine category from tab
                    string category = "Push";
                    if (PullTab.IsSelected) category = "Pull";
                    else if (LegsTab.IsSelected) category = "Legs";

                    // Pre-fill workout details
                    addWorkoutWindow.PreFillExercise(exerciseName, category);

                    if (addWorkoutWindow.ShowDialog() == true && addWorkoutWindow.NewWorkout != null)
                    {
                        // Make sure ID is set
                        if (string.IsNullOrEmpty(addWorkoutWindow.NewWorkout.Id))
                        {
                            addWorkoutWindow.NewWorkout.Id = ObjectId.GenerateNewId().ToString();
                        }

                        // Add to collection and save to database
                        Workouts.Insert(0, addWorkoutWindow.NewWorkout); // Add at the top
                        _workoutDB.InsertWorkout(addWorkoutWindow.NewWorkout);

                        // After logging a workout, return to the workouts tab
                        ShowTab("WorkoutsTab");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error logging workout: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
