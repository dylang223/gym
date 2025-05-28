using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace gym
{
    public partial class ProgressWindow : Window
    {
        private WorkoutDB _workoutDB;
        private List<Workout> _allWorkouts = new();
        private List<Workout> _filteredWorkouts = new();

        // Chart properties
        public List<ISeries> Series { get; set; } = new();
        public List<Axis> XAxes { get; set; } = new();
        public List<Axis> YAxes { get; set; } = new();

        public ProgressWindow()
        {
            InitializeComponent();
            DataContext = this;
            _workoutDB = new WorkoutDB();

            // Set up default axes
            XAxes = new List<Axis>
            {
                new Axis { Name = "Date", Labels = new List<string> { "No Data" } }
            };
            YAxes = new List<Axis>
            {
                new Axis { Name = "Reps/Weight" }
            };

            Loaded += async (_, __) => await LoadDataFromDatabase();
        }

        private async Task LoadDataFromDatabase()
        {
            try
            {
                _allWorkouts = await _workoutDB.GetAllWorkoutsAsync() ?? new List<Workout>();
                _filteredWorkouts = _allWorkouts.ToList();

                if (_allWorkouts.Count == 0)
                {
                    MessageBox.Show("No workout data available. Please add some workouts first.", "No Data",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateChart();
                    UpdateSummary();
                    return;
                }

                PopulateFilters();
                UpdateChart();
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data from database: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _allWorkouts = new List<Workout>();
                _filteredWorkouts = new List<Workout>();
                UpdateChart();
                UpdateSummary();
            }
        }

        private void PopulateFilters()
        {
            if (CategoryFilter == null || ExerciseFilter == null) return;
            CategoryFilter.Items.Clear();
            ExerciseFilter.Items.Clear();

            // Populate category filter
            var categories = _allWorkouts.Select(w => w.Category).Distinct().OrderBy(c => c).ToList();
            CategoryFilter.Items.Add("All Categories");
            foreach (var category in categories)
                CategoryFilter.Items.Add(category);
            CategoryFilter.SelectedIndex = 0;

            // Populate exercise filter
            var exercises = _allWorkouts.Select(w => w.Exercise).Distinct().OrderBy(e => e).ToList();
            ExerciseFilter.Items.Add("All Exercises");
            foreach (var exercise in exercises)
                ExerciseFilter.Items.Add(exercise);
            ExerciseFilter.SelectedIndex = 0;
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.SelectedIndex = 0;
            ExerciseFilter.SelectedIndex = 0;
            DateRangeFilter.SelectedIndex = 3; // "All time"
            _filteredWorkouts = _allWorkouts.ToList();
            UpdateChart();
            UpdateSummary();
        }

        private void ApplyFilters()
        {
            _filteredWorkouts = _allWorkouts.ToList();

            // Category filter
            if (CategoryFilter.SelectedIndex > 0)
            {
                string selectedCategory = CategoryFilter.SelectedItem?.ToString();
                _filteredWorkouts = _filteredWorkouts
                    .Where(w => w.Category == selectedCategory)
                    .ToList();
            }

            // Exercise filter
            if (ExerciseFilter.SelectedIndex > 0)
            {
                string selectedExercise = ExerciseFilter.SelectedItem?.ToString();
                _filteredWorkouts = _filteredWorkouts
                    .Where(w => w.Exercise == selectedExercise)
                    .ToList();
            }

            // Date range filter
            if (DateRangeFilter.SelectedIndex != 3) // Not "All time"
            {
                DateTime cutoffDate = DateTime.Now;
                switch (DateRangeFilter.SelectedIndex)
                {
                    case 0: cutoffDate = DateTime.Now.AddDays(-7); break;
                    case 1: cutoffDate = DateTime.Now.AddDays(-30); break;
                    case 2: cutoffDate = DateTime.Now.AddDays(-90); break;
                }
                _filteredWorkouts = _filteredWorkouts
                    .Where(w => w.Date >= cutoffDate)
                    .ToList();
            }

            UpdateChart();
            UpdateSummary();
        }

        private void UpdateChart()
        {
            if (_filteredWorkouts == null || _filteredWorkouts.Count == 0)
            {
                Series = new List<ISeries>();
                XAxes = new List<Axis>
                {
                    new Axis { Name = "Date", Labels = new List<string> { "No Data" }, TextSize = 14 }
                };
                YAxes = new List<Axis>
                {
                    new Axis { Name = "Reps/Weight", TextSize = 14 }
                };
                DataContext = null;
                DataContext = this;
                return;
            }

            var sortedWorkouts = _filteredWorkouts.OrderBy(w => w.Date).ToList();
            var dates = sortedWorkouts.Select(w => w.Date.ToShortDateString()).Distinct().ToList();
            var groupedWorkouts = sortedWorkouts.GroupBy(w => w.Exercise);

            Series = new List<ISeries>();
            var colors = new List<SKColor>
            {
                SKColors.DodgerBlue, SKColors.Crimson, SKColors.ForestGreen,
                SKColors.Orange, SKColors.Purple, SKColors.DeepPink, SKColors.Teal,
                SKColors.Gold, SKColors.CornflowerBlue, SKColors.HotPink, SKColors.LimeGreen
            };
            int colorIndex = 0;

            foreach (var group in groupedWorkouts)
            {
                var exerciseData = group.ToList();

                // If only one data point exists, duplicate it to create a visible line
                if (exerciseData.Count == 1)
                {
                    var singlePoint = exerciseData[0];
                    var duplicatePoint = new Workout
                    {
                        Id = singlePoint.Id,
                        Category = singlePoint.Category,
                        Exercise = singlePoint.Exercise,
                        Reps = singlePoint.Reps,
                        Sets = singlePoint.Sets,
                        Weight = singlePoint.Weight,
                        Date = singlePoint.Date.AddDays(7) // Add a week to create line
                    };
                    exerciseData.Add(duplicatePoint);

                    // Add date if it doesn't exist
                    if (!dates.Contains(duplicatePoint.Date.ToShortDateString()))
                        dates.Add(duplicatePoint.Date.ToShortDateString());
                }

                // Sort dates array
                dates = dates.OrderBy(d => DateTime.Parse(d)).ToList();

                var repsValues = new List<double?>();
                var weightValues = new List<double?>();

                foreach (var date in dates)
                {
                    var workoutOnDate = exerciseData.FirstOrDefault(w => w.Date.ToShortDateString() == date);

                    // Instead of null values which break lines, use dummy values
                    if (workoutOnDate != null)
                    {
                        repsValues.Add((double)workoutOnDate.Reps);
                        weightValues.Add(workoutOnDate.Weight);
                    }
                    else if (repsValues.Count > 0) // Use last value to maintain line
                    {
                        // Use interpolation to keep lines connected
                        double? lastReps = repsValues.Last();
                        double? lastWeight = weightValues.Last();

                        repsValues.Add(lastReps);
                        weightValues.Add(lastWeight);
                    }
                    else
                    {
                        // First point with no data
                        repsValues.Add(null);
                        weightValues.Add(null);
                    }
                }

                // Reps series with enhanced visibility
                Series.Add(new LineSeries<double?>
                {
                    Name = $"{group.Key} (Reps)",
                    Values = repsValues,
                    GeometrySize = 10, // Size of dots
                    Stroke = new SolidColorPaint(colors[colorIndex % colors.Count]) { StrokeThickness = 4 }, // Thicker lines
                    GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(colors[colorIndex % colors.Count]),
                    Fill = null,
                    LineSmoothness = 0.7 // Smoother curves between points
                });
                colorIndex++;

                // Weight series with enhanced visibility
                Series.Add(new LineSeries<double?>
                {
                    Name = $"{group.Key} (Weight)",
                    Values = weightValues,
                    GeometrySize = 10,
                    Stroke = new SolidColorPaint(colors[colorIndex % colors.Count]) { StrokeThickness = 4 }, // Thicker lines
                    GeometryFill = new SolidColorPaint(colors[colorIndex % colors.Count]),
                    GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    Fill = null,
                    LineSmoothness = 0.7 // Smoother curves between points
                });
                colorIndex++;
            }

            // Enhanced axis visualization for both normal and fullscreen modes
            XAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Date",
                    Labels = dates,
                    LabelsRotation = 45,
                    TextSize = 14, // Larger text for full screen
                    NamePaint = new SolidColorPaint(SKColors.DarkSlateGray), // Better color
                    LabelsPaint = new SolidColorPaint(SKColors.DarkSlateGray)
                }
            };

            YAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Reps/Weight",
                    TextSize = 14, // Larger text for full screen
                    NamePaint = new SolidColorPaint(SKColors.DarkSlateGray), // Better color
                    LabelsPaint = new SolidColorPaint(SKColors.DarkSlateGray)
                }
            };

            // Force chart to update
            DataContext = null;
            DataContext = this;
        }

        private void UpdateSummary()
        {
            if (SummaryText == null) return;
            if (_filteredWorkouts == null || _filteredWorkouts.Count == 0)
            {
                SummaryText.Text = "No data to display.";
                return;
            }
            // ... rest of your code
        }
    }
}
