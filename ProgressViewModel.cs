// ProgressViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace gym
{
    public class ProgressViewModel : ViewModelBase
    {
        private readonly IWorkoutRepository _workoutDB;
        private List<Workout> _allWorkouts = new();
        private List<Workout> _filteredWorkouts = new();
        private bool _isLoading;

        // Properties for binding
        public ObservableCollection<string> Categories { get; } = new();
        public ObservableCollection<string> Exercises { get; } = new();

        private string _selectedCategory = "All Categories";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    ApplyFilters();
                }
            }
        }

        private string _selectedExercise = "All Exercises";
        public string SelectedExercise
        {
            get => _selectedExercise;
            set
            {
                if (SetProperty(ref _selectedExercise, value))
                {
                    ApplyFilters();
                }
            }
        }

        private int _selectedDateRange = 3; // Default to "All time"
        public int SelectedDateRange
        {
            get => _selectedDateRange;
            set
            {
                if (SetProperty(ref _selectedDateRange, value))
                {
                    ApplyFilters();
                }
            }
        }

        // Add private backing fields with property change notification
        private List<ISeries> _series = new();
        public List<ISeries> Series
        {
            get => _series;
            set => SetProperty(ref _series, value);
        }

        private List<Axis> _xAxes = new();
        public List<Axis> XAxes
        {
            get => _xAxes;
            set => SetProperty(ref _xAxes, value);
        }

        private List<Axis> _yAxes = new();
        public List<Axis> YAxes
        {
            get => _yAxes;
            set => SetProperty(ref _yAxes, value);
        }

        private string _summaryText = "No data to display.";
        public string SummaryText
        {
            get => _summaryText;
            set => SetProperty(ref _summaryText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Command for reset filters button
        private RelayCommand _resetFiltersCommand;
        public ICommand ResetFiltersCommand => _resetFiltersCommand ??= new RelayCommand(_ => ResetFilters());

        public ProgressViewModel(IWorkoutRepository repository = null)
        {
            // Allow dependency injection but fall back to direct instantiation if needed
            _workoutDB = repository ?? new WorkoutDB();

            // Initialize chart axes
            XAxes = new List<Axis>
            {
                new Axis { Name = "Date", Labels = new List<string> { "No Data" } }
            };

            YAxes = new List<Axis>
            {
                new Axis { Name = "Reps/Weight" }
            };
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                _allWorkouts = await _workoutDB.GetAllWorkoutsAsync() ?? new List<Workout>();
                _filteredWorkouts = _allWorkouts.ToList();

                if (_allWorkouts.Count == 0)
                {
                    SummaryText = "No data to display.";
                    return;
                }

                PopulateFilters();
                UpdateChart();
                UpdateSummary();
            }
            catch (Exception ex)
            {
                SummaryText = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void PopulateFilters()
        {
            Categories.Clear();
            Exercises.Clear();

            Categories.Add("All Categories");
            foreach (var category in _allWorkouts.Select(w => w.Category).Distinct().OrderBy(c => c))
                Categories.Add(category);

            Exercises.Add("All Exercises");
            foreach (var exercise in _allWorkouts.Select(w => w.Exercise).Distinct().OrderBy(e => e))
                Exercises.Add(exercise);
        }

        private void ResetFilters()
        {
            SelectedCategory = "All Categories";
            SelectedExercise = "All Exercises";
            SelectedDateRange = 3; // All time

            _filteredWorkouts = _allWorkouts.ToList();
            UpdateChart();
            UpdateSummary();
        }

        private void ApplyFilters()
        {
            _filteredWorkouts = _allWorkouts.ToList();

            // Category filter
            if (SelectedCategory != "All Categories")
            {
                _filteredWorkouts = _filteredWorkouts
                    .Where(w => w.Category == SelectedCategory)
                    .ToList();
            }

            // Exercise filter
            if (SelectedExercise != "All Exercises")
            {
                _filteredWorkouts = _filteredWorkouts
                    .Where(w => w.Exercise == SelectedExercise)
                    .ToList();
            }

            // Date range filter
            if (SelectedDateRange != 3) // Not "All time"
            {
                DateTime cutoffDate = DateTime.Now;
                switch (SelectedDateRange)
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
                return;
            }

            var sortedWorkouts = _filteredWorkouts.OrderBy(w => w.Date).ToList();
            var dates = sortedWorkouts.Select(w => w.Date.ToShortDateString()).Distinct().ToList();
            var groupedWorkouts = sortedWorkouts.GroupBy(w => w.Exercise);

            var newSeries = new List<ISeries>();
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
                    var workoutOnDate = exerciseData
                        .Where(w => w.Date.ToShortDateString() == date)
                        .OrderByDescending(w => w.Weight)
                        .FirstOrDefault();

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
                newSeries.Add(new LiveChartsCore.SkiaSharpView.LineSeries<double?>
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
                newSeries.Add(new LiveChartsCore.SkiaSharpView.LineSeries<double?>
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

            // Update the Series property to trigger property change notification
            Series = newSeries;
        }

        private void UpdateSummary()
        {
            if (_filteredWorkouts == null || _filteredWorkouts.Count == 0)
            {
                SummaryText = "No data to display.";
                return;
            }

            int totalWorkouts = _filteredWorkouts.Count;
            double totalVolume = _filteredWorkouts.Sum(w => w.Reps * w.Sets * w.Weight);
            double avgReps = _filteredWorkouts.Average(w => w.Reps);
            double avgWeight = _filteredWorkouts.Average(w => w.Weight);
            var maxWeightByExercise = _filteredWorkouts
                .GroupBy(w => w.Exercise)
                .Select(g => $"{g.Key}: {g.Max(w => w.Weight)}kg");
            DateTime lastWorkout = _filteredWorkouts.Max(w => w.Date);

            SummaryText =
                $"Total Workouts: {totalWorkouts}\n" +
                $"Total Volume: {totalVolume:F1}\n" +
                $"Average Reps: {avgReps:F1}\n" +
                $"Average Weight: {avgWeight:F1}kg\n" +
                $"Best Weight per Exercise:\n  {string.Join("\n  ", maxWeightByExercise)}\n" +
                $"Most Recent Workout: {lastWorkout:d}";
        }
    }

    // Simple RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
