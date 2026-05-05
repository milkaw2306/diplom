using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diplom_zxc.Services;
using Microsoft.Win32;

namespace Diplom_zxc.ViewModels
{
    public partial class ExportViewModel : BaseViewModel
    {
        private readonly ExportService _exportService;

        [ObservableProperty]
        private string _exportPath = string.Empty;

        [ObservableProperty]
        private bool _isExporting;

        [ObservableProperty]
        private ObservableCollection<ExportOption> _exportTypes = new();

        [ObservableProperty]
        private ExportOption? _selectedExportType;

        public ExportViewModel()
        {
            _exportService = new ExportService(App.ConnectionString);
            Title = "Экспорт фотографий";

            ExportTypes = new ObservableCollection<ExportOption>
            {
                new ExportOption("Оригинальное качество", "original"),
                new ExportOption("Сжатое (для web)", "compressed"),
                new ExportOption("С водяным знаком", "watermarked")
            };

            SelectedExportType = ExportTypes[0];

            ExportCommand = new AsyncRelayCommand(ExportAsync);
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
        }

        public ICommand ExportCommand { get; }
        public ICommand BrowseFolderCommand { get; }

        private async Task ExportAsync()
        {
            if (string.IsNullOrEmpty(ExportPath))
            {
                MessageBox.Show("Выберите папку для экспорта");
                return;
            }

            try
            {
                IsExporting = true;

                // В реальном приложении здесь будут выбранные фото
                var photoIds = new List<int>();

                bool success = await _exportService.ExportPhotos(photoIds, ExportPath);

                if (success)
                {
                    MessageBox.Show("Экспорт завершен успешно!", "Экспорт",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsExporting = false;
            }
        }

        private void BrowseFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Выберите папку для экспорта"
            };

            if (dialog.ShowDialog() == true)
            {
                ExportPath = dialog.FolderName;
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Views.ExportWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }

    public class ExportOption
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public ExportOption(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}