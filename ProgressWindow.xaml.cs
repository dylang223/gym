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
        public List<string> Dates { get; set; } 

        public ProgressWindow(List<Workout> workouts)
        {
            InitializeComponent();

            if (workouts == null || !workouts.Any())
            {
                MessageBox.Show("No workout data available to display progress.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

           
            var sortedWorkouts = workouts.OrderBy(w => w.Date).ToList();

            Dates = sortedWorkouts.Select(w => w.Date.ToShortDateString()).Distinct().ToList();

         
            var groupedWorkouts = sortedWorkouts.GroupBy(w => w.Exercise);

      
            var seriesCollection = new SeriesCollection();

          
            foreach (var group in groupedWorkouts)
            {
              
                var repsSeries = new LineSeries
                {
                    Title = $"{group.Key} (Reps)",
                    Values = new ChartValues<double>(group.Select(w => (double)w.Reps)),
                    PointGeometrySize = 10
                };

               
                var weightSeries = new LineSeries
                {
                    Title = $"{group.Key} (Weight)",
                    Values = new ChartValues<double>(group.Select(w => w.Weight)),
                    PointGeometrySize = 10,
                    Stroke = System.Windows.Media.Brushes.Red 
                };

              
                seriesCollection.Add(repsSeries);
                seriesCollection.Add(weightSeries);
            }

          
            ProgressChart.Series = seriesCollection;

          
            ProgressChart.AxisX[0].Labels = Dates;
        }
    }
}
