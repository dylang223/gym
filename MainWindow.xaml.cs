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
                LoadDataAndStatsAsync();
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

        // Renamed to avoid ambiguity
        private async void LoadDataAndStatsAsync()
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

                    // Update the stats after loading data
                    UpdateWorkoutStats();
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

                    // Update stats after adding a new workout
                    UpdateWorkoutStats();

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

        private void UpdateWorkoutStats()
        {
            try
            {
                // Create a list of workout statistics
                var stats = new ObservableCollection<WorkoutStat>();

                // Only process if we have workouts
                if (Workouts.Count > 0)
                {
                    // Total workouts
                    stats.Add(new WorkoutStat
                    {
                        StatName = "Total Workouts",
                        Value = Workouts.Count.ToString(),
                        Description = "Total number of workouts recorded",
                        Icon = "📝"
                    });

                    // Most recent workout
                    var mostRecent = Workouts.OrderByDescending(w => w.Date).FirstOrDefault();
                    stats.Add(new WorkoutStat
                    {
                        StatName = "Most Recent Workout",
                        Value = mostRecent?.Date.ToString("MM/dd/yyyy"),
                        Description = $"{mostRecent?.Exercise} ({mostRecent?.Category})",
                        Icon = "🔄"
                    });

                    // Calculate total volume
                    double totalVolume = Workouts.Sum(w => w.Sets * w.Reps * w.Weight);
                    stats.Add(new WorkoutStat
                    {
                        StatName = "Total Volume Lifted",
                        Value = $"{totalVolume:N0} kg",
                        Description = "Sum of (sets × reps × weight) across all workouts",
                        Icon = "💪"
                    });

                    // Most frequent exercise
                    var mostFrequentExercise = Workouts
                        .GroupBy(w => w.Exercise)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault();
                    stats.Add(new WorkoutStat
                    {
                        StatName = "Most Frequent Exercise",
                        Value = mostFrequentExercise?.Key ?? "N/A",
                        Description = $"Performed {mostFrequentExercise?.Count() ?? 0} times",
                        Icon = "🏆"
                    });

                    // Heaviest workout
                    var heaviestWorkout = Workouts.OrderByDescending(w => w.Weight).FirstOrDefault();
                    stats.Add(new WorkoutStat
                    {
                        StatName = "Heaviest Weight Lifted",
                        Value = $"{heaviestWorkout?.Weight:F1} kg",
                        Description = $"{heaviestWorkout?.Exercise} on {heaviestWorkout?.Date.ToString("MM/dd/yyyy")}",
                        Icon = "🏋️‍♂️"
                    });

                    // Most improved exercise
                    var exercises = Workouts
                        .GroupBy(w => w.Exercise)
                        .Where(g => g.Count() >= 2)
                        .Select(g => new
                        {
                            Exercise = g.Key,
                            FirstWorkout = g.OrderBy(w => w.Date).First(),
                            LastWorkout = g.OrderBy(w => w.Date).Last(),
                        })
                        .Select(e => new
                        {
                            e.Exercise,
                            WeightGain = e.LastWorkout.Weight - e.FirstWorkout.Weight,
                            FirstDate = e.FirstWorkout.Date,
                            LastDate = e.LastWorkout.Date
                        })
                        .OrderByDescending(e => e.WeightGain)
                        .FirstOrDefault();

                    if (exercises != null)
                    {
                        stats.Add(new WorkoutStat
                        {
                            StatName = "Most Improved Exercise",
                            Value = $"+{exercises.WeightGain:F1} kg",
                            Description = $"{exercises.Exercise} - from {exercises.FirstDate:MM/dd} to {exercises.LastDate:MM/dd}",
                            Icon = "📈"
                        });
                    }

                    // Category breakdown
                    var categoryCount = Workouts
                        .GroupBy(w => w.Category)
                        .Select(g => new { Category = g.Key, Count = g.Count() })
                        .OrderByDescending(c => c.Count)
                        .FirstOrDefault();

                    if (categoryCount != null)
                    {
                        stats.Add(new WorkoutStat
                        {
                            StatName = "Most Trained Category",
                            Value = categoryCount.Category,
                            Description = $"Trained {categoryCount.Count} times ({(100.0 * categoryCount.Count / Workouts.Count):F0}% of all workouts)",
                            Icon = "🎯"
                        });
                    }

                    // Average workouts per week
                    if (Workouts.Count >= 2)
                    {
                        var firstDate = Workouts.Min(w => w.Date);
                        var lastDate = Workouts.Max(w => w.Date);
                        var totalWeeks = Math.Max(1, (lastDate - firstDate).TotalDays / 7.0);
                        var workoutsPerWeek = Workouts.Count / totalWeeks;

                        stats.Add(new WorkoutStat
                        {
                            StatName = "Workouts per Week",
                            Value = $"{workoutsPerWeek:F1}",
                            Description = $"Average workouts per week over {totalWeeks:F0} weeks",
                            Icon = "📅"
                        });
                    }
                }
                else
                {
                    // No workouts yet
                    stats.Add(new WorkoutStat
                    {
                        StatName = "No Workouts",
                        Value = "0",
                        Description = "Add your first workout to see statistics",
                        Icon = "➕"
                    });
                }

                // Bind to the ItemsControl
                StatsItemsControl.ItemsSource = stats;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating workout statistics: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
