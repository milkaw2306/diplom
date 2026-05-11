using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diplom_zxc.Services;
using Microsoft.Win32;
using System.Windows;

namespace Diplom_zxc.ViewModels
{
    public partial class ImportViewModel : ObservableObject
    {
        private readonly ImportService _importService;

        [ObservableProperty]
        private string _statusText = "Готов к импорту";

        [ObservableProperty]
        private bool _isImporting;

        [ObservableProperty]
        private string _currentFile = string.Empty;

        [ObservableProperty]
        private int _progressValue;

        public ImportViewModel()
        {
            _importService = new ImportService(App.ConnectionString, App.CurrentUserId);
            _importService.ProgressChanged += OnProgressChanged;
            _importService.ImportCompleted += OnImportCompleted;
        }

        private void OnProgressChanged(object? sender, ImportProgressEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentFile = e.CurrentFile ?? "";
                ProgressValue = e.ProcessedCount;
                StatusText = $"Импорт: {e.CurrentFile}";
            });
        }

        private void OnImportCompleted(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsImporting = false;
                StatusText = "Импорт завершен!";
            });
        }

        [RelayCommand]
        private void ImportFolder()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                IsImporting = true;
                _ = _importService.ImportFolderWithStructure(dialog.FolderName);
            }
        }

        [RelayCommand]
        private void ImportFiles()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                IsImporting = true;
                _ = _importService.ImportDraggedFiles(dialog.FileNames, 0);
            }
        }
    }
}   