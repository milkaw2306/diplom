using System;
using System.Windows;
using System.Windows.Controls;
using Diplom_zxc.ViewModels;

namespace Diplom_zxc.Views
{
    /// <summary>
    /// Логика взаимодействия для ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private readonly ExportViewModel? _viewModel;

        public ExportWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as ExportViewModel;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && !string.IsNullOrEmpty(_viewModel.ExportPath))
            {
                _viewModel.ExportCommand.Execute(null);
            }
        }
    }
}