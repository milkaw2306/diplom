using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diplom_zxc.Models;
using Diplom_zxc.Services;
using MailKit.Search;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static MaterialDesignThemes.Wpf.Theme;

namespace Diplom_zxc.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly FileService _fileService;
        private readonly ImportService _importService;
        private readonly ExportService _exportService;

        // Коллекции данных
        [ObservableProperty]
        private ObservableCollection<Folder> _folders = new();

        [ObservableProperty]
        private ObservableCollection<Photo> _photos = new();

        [ObservableProperty]
        private ObservableCollection<Photo> _selectedPhotos = new();

        // Выбранные элементы
        [ObservableProperty]
        private Folder? _selectedFolder;

        [ObservableProperty]
        private Photo? _selectedPhoto;

        // Навигация
        [ObservableProperty]
        private string _currentPath = "📁 Мой диск";

        [ObservableProperty]
        private string _breadcrumbPath = "📁 Мой диск";

        [ObservableProperty]
        private Stack<int?> _navigationHistory = new();

        [ObservableProperty]
        private bool _canGoBack;

        // Хранилище
        [ObservableProperty]
        private long _storageUsed;

        [ObservableProperty]
        private long _storageLimit = 10737418240;

        [ObservableProperty]
        private double _storagePercent;

        [ObservableProperty]
        private string _storageDisplay = "0 B / 10 GB";

        // Состояние UI
        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isGridView = true;

        [ObservableProperty]
        private bool _isListView;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _statusText = "Готов";

        [ObservableProperty]
        private int _totalPhotosCount;

        [ObservableProperty]
        private int _totalFoldersCount;

        // Информация о пользователе
        [ObservableProperty]
        private string _userName = App.CurrentUsername ?? "Пользователь";

        [ObservableProperty]
        private string _userEmail = string.Empty;

        // Свойства для диалогов
        [ObservableProperty]
        private bool _isCreateFolderDialogOpen;

        [ObservableProperty]
        private string _newFolderName = "Новая папка";

        public MainViewModel()
        {
            _fileService = new FileService(App.ConnectionString);
            _importService = new ImportService(App.ConnectionString, App.CurrentUserId);
            _exportService = new ExportService(App.ConnectionString);

            // Инициализация данных
            _ = LoadDataAsync();
            UpdateStorageDisplay();
        }

        // ========== КОМАНДЫ НАВИГАЦИИ ==========

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = "Загрузка данных...";

                var folders = await _fileService.GetFoldersAsync(App.CurrentUserId);
                Folders = new ObservableCollection<Folder>(folders);

                if (SelectedFolder != null)
                {
                    await LoadPhotosForFolderAsync(SelectedFolder);
                }

                UpdateCounters();
                UpdateStorageInfo();
                StatusText = "Готов";
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка загрузки данных";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToFolderAsync(Folder? folder)
        {
            if (folder == null) return;

            try
            {
                IsLoading = true;
                StatusText = $"Открытие папки: {folder.FolderName}";

                // Сохраняем историю
                NavigationHistory.Push(SelectedFolder?.FolderId);
                CanGoBack = true;

                SelectedFolder = folder;

                // Загружаем подпапки и фото
                var subFolders = await _fileService.GetFoldersAsync(App.CurrentUserId, folder.FolderId);
                Folders = new ObservableCollection<Folder>(subFolders);

                await LoadPhotosForFolderAsync(folder);

                // Обновляем хлебные крошки
                BreadcrumbPath = await BuildBreadcrumbAsync(folder);
                CurrentPath = folder.FolderName ?? "Папка";

                UpdateCounters();
                StatusText = $"Папка: {folder.FolderName}";
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка навигации";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            if (!CanGoBack || NavigationHistory.Count == 0) return;

            try
            {
                IsLoading = true;
                int? previousFolderId = NavigationHistory.Pop();
                CanGoBack = NavigationHistory.Count > 0;

                if (previousFolderId == null)
                {
                    // Возврат в корень
                    SelectedFolder = null;
                    CurrentPath = "📁 Мой диск";
                    BreadcrumbPath = "📁 Мой диск";
                    await LoadDataAsync();
                }
                else
                {
                    var folders = await _fileService.GetFoldersAsync(App.CurrentUserId);
                    var folder = FindFolderById(folders, previousFolderId.Value);
                    if (folder != null)
                    {
                        await NavigateToFolderAsync(folder);
                    }
                }

                StatusText = "Готов";
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка возврата";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (SelectedFolder != null)
            {
                await NavigateToFolderAsync(SelectedFolder);
            }
            else
            {
                await LoadDataAsync();
            }
        }

        // ========== КОМАНДЫ ДЛЯ ПАПОК ==========

        [RelayCommand]
        private async Task CreateFolderAsync()
        {
            if (string.IsNullOrWhiteSpace(NewFolderName)) return;

            try
            {
                IsLoading = true;
                StatusText = "Создание папки...";

                int? parentId = SelectedFolder?.FolderId;
                await _fileService.CreateFolderAsync(App.CurrentUserId, NewFolderName, parentId);

                NewFolderName = "Новая папка";
                IsCreateFolderDialogOpen = false;

                await RefreshAsync();
                StatusText = "Папка создана";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания папки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Ошибка создания папки";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RenameFolderAsync(Folder? folder)
        {
            if (folder == null) return;

            string? newName = Microsoft.VisualBasic.Interaction.InputBox(
                "Новое имя папки:", "Переименование", folder.FolderName);

            if (!string.IsNullOrWhiteSpace(newName))
            {
                // Обновляем имя папки
                folder.FolderName = newName;
                await RefreshAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteFolderAsync(Folder? folder)
        {
            if (folder == null) return;

            var result = MessageBox.Show(
                $"Удалить папку \"{folder.FolderName}\" со всем содержимым?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _fileService.DeleteFolderAsync(folder.FolderId);
                    Folders.Remove(folder);
                    UpdateStorageInfo();
                    StatusText = "Папка удалена";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ========== КОМАНДЫ ДЛЯ ФОТО ==========

        [RelayCommand]
        private async Task DeletePhotoAsync(Photo? photo)
        {
            if (photo == null) return;

            var result = MessageBox.Show(
                $"Удалить фото \"{photo.OriginalName}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _fileService.DeletePhotoAsync(photo.PhotoId);
                    Photos.Remove(photo);
                    SelectedPhotos.Remove(photo);

                    UpdateCounters();
                    UpdateStorageInfo();
                    StatusText = "Фото удалено";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task DeleteSelectedPhotosAsync()
        {
            if (SelectedPhotos.Count == 0)
            {
                MessageBox.Show("Выберите фото для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить {SelectedPhotos.Count} выбранных фото?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var photo in SelectedPhotos.ToList())
                {
                    await _fileService.DeletePhotoAsync(photo.PhotoId);
                    Photos.Remove(photo);
                }
                SelectedPhotos.Clear();
                UpdateCounters();
                UpdateStorageInfo();
            }
        }

        [RelayCommand]
        private void SelectPhoto(Photo? photo)
        {
            if (photo == null) return;

            if (SelectedPhotos.Contains(photo))
            {
                SelectedPhotos.Remove(photo);
            }
            else
            {
                SelectedPhotos.Add(photo);
            }

            StatusText = SelectedPhotos.Count > 0
                ? $"Выбрано: {SelectedPhotos.Count}"
                : "Готов";
        }

        [RelayCommand]
        private void SelectAllPhotos()
        {
            SelectedPhotos = new ObservableCollection<Photo>(Photos);
            StatusText = $"Выбрано все: {Photos.Count}";
        }

        [RelayCommand]
        private void ClearSelection()
        {
            SelectedPhotos.Clear();
            StatusText = "Выбор снят";
        }

        // ========== КОМАНДЫ ИМПОРТА/ЭКСПОРТА ==========

        [RelayCommand]
        private void OpenImportWindow()
        {
            var importWindow = new Views.ImportWindow();
            importWindow.Owner = Application.Current.MainWindow;

            if (importWindow.ShowDialog() == true)
            {
                _ = RefreshAsync();
            }
        }

        [RelayCommand]
        private void OpenExportWindow()
        {
            if (SelectedPhotos.Count == 0 && Photos.Count == 0)
            {
                MessageBox.Show("Нет фото для экспорта", "Экспорт",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var exportWindow = new Views.ExportWindow();
            exportWindow.Owner = Application.Current.MainWindow;
            exportWindow.ShowDialog();
        }

        // ========== КОМАНДЫ ШАРИНГА ==========

        [RelayCommand]
        private void ShareSelectedFolder()
        {
            if (SelectedFolder != null)
            {
                Views.ShareDialog.ShowShareDialog(SelectedFolder);
            }
        }

        [RelayCommand]
        private void ShareSelectedPhoto()
        {
            if (SelectedPhoto != null)
            {
                Views.ShareDialog.ShowShareDialog(SelectedPhoto);
            }
        }

        [RelayCommand]
        private void ShareSelectedPhotos()
        {
            if (SelectedPhotos.Count > 0)
            {
                Views.ShareDialog.ShowShareDialog(SelectedPhotos.First());
            }
            else if (SelectedFolder != null)
            {
                Views.ShareDialog.ShowShareDialog(SelectedFolder);
            }
        }

        // ========== КОМАНДЫ ОТОБРАЖЕНИЯ ==========

        [RelayCommand]
        private void SwitchToGridView()
        {
            IsGridView = true;
            IsListView = false;
        }

        [RelayCommand]
        private void SwitchToListView()
        {
            IsGridView = false;
            IsListView = true;
        }

        [RelayCommand]
        private void ToggleView()
        {
            IsGridView = !IsGridView;
            IsListView = !IsGridView;
        }

        // ========== КОМАНДЫ ПОИСКА ==========

        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                _ = RefreshAsync();
                return;
            }

            var filtered = Photos.Where(p =>
                (p.OriginalName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (p.FileName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)
            ).ToList();

            Photos = new ObservableCollection<Photo>(filtered);
            StatusText = $"Найдено: {filtered.Count}";
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
            _ = RefreshAsync();
        }

        // ========== СИСТЕМНЫЕ КОМАНДЫ ==========

        [RelayCommand]
        private void Logout()
        {
            var result = MessageBox.Show("Выйти из системы?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                App.CurrentUserId = 0;
                App.CurrentUsername = null;

                var loginWindow = new Views.LoginWindow();
                loginWindow.Show();

                Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w is Views.MainWindow)?.Close();
            }
        }

        [RelayCommand]
        private void ShowAboutInfo()
        {
            MessageBox.Show(
                "Diplom_zxc v1.0\n\n" +
                "Приложение для портфолио фотографов\n" +
                "Дипломный проект\n\n" +
                "Функции:\n" +
                "• Управление папками как в Яндекс.Диске\n" +
                "• Красивое отображение плитки фотографий\n" +
                "• Импорт и экспорт фото\n" +
                "• Шаринг с другими пользователями\n\n" +
                "© 2026 Все права защищены",
                "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

        private async Task LoadPhotosForFolderAsync(Folder folder)
        {
            var photos = await _fileService.GetPhotosInFolderAsync(folder.FolderId);
            Photos = new ObservableCollection<Photo>(photos);
        }

        private async Task<string> BuildBreadcrumbAsync(Folder folder)
        {
            var parts = new System.Collections.Generic.List<string>();
            var current = folder;

            while (current != null)
            {
                parts.Insert(0, current.FolderName ?? "Папка");

                if (current.ParentFolderId != null)
                {
                    var allFolders = await _fileService.GetFoldersAsync(App.CurrentUserId);
                    current = allFolders.FirstOrDefault(f => f.FolderId == current.ParentFolderId);
                }
                else
                {
                    break;
                }
            }

            parts.Insert(0, "📁 Мой диск");
            return string.Join(" > ", parts);
        }

        private Folder? FindFolderById(IEnumerable<Folder> folders, int folderId)
        {
            foreach (var folder in folders)
            {
                if (folder.FolderId == folderId) return folder;
            }
            return null;
        }

        private void UpdateCounters()
        {
            TotalPhotosCount = Photos.Count;
            TotalFoldersCount = Folders.Count;
        }

        private async void UpdateStorageInfo()
        {
            try
            {
                using var connection = new MySql.Data.MySqlClient.MySqlConnection(App.ConnectionString);
                await connection.OpenAsync();

                var user = await Dapper.SqlMapper.QueryFirstOrDefaultAsync<dynamic>(
                    connection,
                    "SELECT StorageLimit, StorageUsed FROM Users WHERE UserId = @UserId",
                    new { UserId = App.CurrentUserId });

                if (user != null)
                {
                    StorageLimit = user.StorageLimit;
                    StorageUsed = user.StorageUsed;
                    StoragePercent = (double)StorageUsed / StorageLimit * 100;
                }

                UpdateStorageDisplay();
            }
            catch { /* Игнорируем ошибки обновления */ }
        }

        private void UpdateStorageDisplay()
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double used = StorageUsed;
            double limit = StorageLimit;
            int usedOrder = 0, limitOrder = 0;

            while (used >= 1024 && usedOrder < sizes.Length - 1)
            {
                usedOrder++;
                used /= 1024;
            }

            while (limit >= 1024 && limitOrder < sizes.Length - 1)
            {
                limitOrder++;
                limit /= 1024;
            }

            StorageDisplay = $"{used:0.0} {sizes[usedOrder]} / {limit:0.0} {sizes[limitOrder]}";
        }

        // Реакция на изменения
        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _ = RefreshAsync();
            }
        }
    }
}