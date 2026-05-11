using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diplom_zxc.Services;
using Microsoft.Win32;

namespace Diplom_zxc.ViewModels
{
    public partial class ExportViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _exportPath = string.Empty;

        [ObservableProperty]
        private bool _isExporting;

        [ObservableProperty]
        private string _statusText = "Готов";

        public ExportViewModel()
        {
        }

        [RelayCommand]
        private void BrowseFolder()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                ExportPath = dialog.FolderName;
            }
        }

        [RelayCommand]
        private void Export()
        {
            if (string.IsNullOrEmpty(ExportPath))
            {
                MessageBox.Show("Выберите папку!");
                return;
            }

            IsExporting = true;
            StatusText = "Экспорт...";

            // Логика экспорта

            IsExporting = false;
            StatusText = "Готово!";
            MessageBox.Show("Экспорт завершен!");
        }
    }
}