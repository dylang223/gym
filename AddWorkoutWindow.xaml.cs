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

namespace gym
{
    /// <summary>
    /// Interaction logic for AddWorkoutWindow.xaml
    /// </summary>
    public partial class AddWorkoutWindow : Window
    {
        public Workout NewWorkout { get; private set; }

        public AddWorkoutWindow()
        {
            InitializeComponent();
        }

        private void AddWorkout_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ExerciseNameTextBox.Text) ||
                !int.TryParse(RepsTextBox.Text, out int reps) ||
                !int.TryParse(SetsTextBox.Text, out int sets) ||
                !double.TryParse(WeightTextBox.Text, out double weight) ||
                WorkoutDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please fill in all fields with valid data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create a new workout object
            NewWorkout = new Workout
            {
                Exercise = ExerciseNameTextBox.Text,
                Reps = reps,
                Sets = sets,
                Weight = weight,
                Date = WorkoutDatePicker.SelectedDate.Value
            };

            // Close the window and return to the main screen
            DialogResult = true;
            Close();
        }
    }
}
