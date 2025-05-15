using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;


namespace gym
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public List<string> Dates { get; set; } // X-axis labels (dates)

        public ProgressWindow(List<Workout> workouts)
        {
            InitializeComponent();

            if (workouts == null || !workouts.Any())
            {
                MessageBox.Show("No workout data available to display progress.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Sort workouts by date
            var sortedWorkouts = workouts.OrderBy(w => w.Date).ToList();

            // Extract unique dates for the X-axis
            Dates = sortedWorkouts.Select(w => w.Date.ToShortDateString()).Distinct().ToList();

            // Group workouts by exercise
            var groupedWorkouts = sortedWorkouts.GroupBy(w => w.Exercise);

            // Create a SeriesCollection
            var seriesCollection = new SeriesCollection();

            // Add a line series for each exercise
            foreach (var group in groupedWorkouts)
            {
                // Create a line series for reps
                var repsSeries = new LineSeries
                {
                    Title = $"{group.Key} (Reps)",
                    Values = new ChartValues<double>(group.Select(w => (double)w.Reps)),
                    PointGeometrySize = 10
                };

                // Create a line series for weight
                var weightSeries = new LineSeries
                {
                    Title = $"{group.Key} (Weight)",
                    Values = new ChartValues<double>(group.Select(w => w.Weight)),
                    PointGeometrySize = 10,
                    Stroke = System.Windows.Media.Brushes.Red // Different color for weight
                };

                // Add the series to the collection
                seriesCollection.Add(repsSeries);
                seriesCollection.Add(weightSeries);
            }

            // Assign the series collection to the chart
            ProgressChart.Series = seriesCollection;

            // Bind the X-axis labels
            ProgressChart.AxisX[0].Labels = Dates;
        }
    }
}
