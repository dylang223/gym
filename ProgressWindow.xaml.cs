// ProgressWindow.xaml.cs
using System.Windows;
using System.Windows.Controls;

namespace gym
{
    public partial class ProgressWindow : Window
    {
        private readonly ProgressViewModel _viewModel;

        public ProgressWindow()
        {
            InitializeComponent();

            // Create ViewModel directly - we can use DI later
            _viewModel = new ProgressViewModel();
            DataContext = _viewModel;

            Loaded += async (s, e) => await _viewModel.LoadDataAsync();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No implementation needed - handled by ViewModel binding
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            // Call command from ViewModel
            _viewModel.ResetFiltersCommand.Execute(null);
        }
    }
}
