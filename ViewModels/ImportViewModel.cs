using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diplom_zxc.Services;
using Microsoft.Win32;

namespace Diplom_zxc.ViewModels
{
    public partial class ImportViewModel : BaseViewModel
    {
        private readonly ImportService _importService;

        [ObservableProperty]
        private string _statusText = "Готов к импорту";

        [ObservableProperty]
        private int _progressValue;

        [ObservableProperty]
        private int _totalFiles;

        [ObservableProperty]
        private bool _isImporting;

        [ObservableProperty]
        private string _currentFile = string.Empty;

        public ImportViewModel()
        {
            _importService = new ImportService(App.ConnectionString, App.CurrentUserId);

            _importService.ProgressChanged += OnImportProgress;
            _importService.ImportCompleted += OnImportCompleted;

            Title = "Импорт фотографий";

            ImportFolderCommand = new AsyncRelayCommand(ImportFolderAsync);
            ImportFilesCommand = new AsyncRelayCommand(ImportFilesAsync);
            CancelImportCommand = new RelayCommand(CancelImport);
        }

        public ICommand ImportFolderCommand { get; }
        public ICommand ImportFilesCommand { get; }
        public ICommand CancelImportCommand { get; }

        private async Task ImportFolderAsync()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Выберите папку с фотографиями для импорта"
            };

            if (dialog.ShowDialog() == true)
            {
                await StartImport(dialog.FolderName);
            }
        }

        private async Task ImportFilesAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите фотографии для импорта",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.webp",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                IsImporting = true;
                StatusText = "Импорт выбранных файлов...";

                int result = await _importService.ImportDraggedFiles(dialog.FileNames, 0);

                MessageBox.Show($"Успешно импортировано {result} файлов!",
                    "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);

                IsImporting = false;
                CloseWindow();
            }
        }

        private async Task StartImport(string folderPath)
        {
            IsImporting = true;
            StatusText = "Начинаем импорт...";

            int result = await _importService.ImportFolderWithStructure(folderPath);

            MessageBox.Show($"Успешно импортировано {result} фотографий!",
                "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);

            IsImporting = false;
            CloseWindow();
        }

        private void CancelImport()
        {
            IsImporting = false;
            StatusText = "Импорт отменен";
        }

        private void OnImportProgress(object? sender, ImportProgressEventArgs e)
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

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Views.ImportWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}