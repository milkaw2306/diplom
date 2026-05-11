using System.Windows;
using System.Windows.Controls;
using Diplom_zxc.ViewModels;
using Diplom_zxc.Models;

namespace Diplom_zxc.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as MainViewModel;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите выйти?",
                "Выход",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
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
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void PhotoContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Photo photo)
            {
                var contextMenu = new ContextMenu();

                var viewItem = new MenuItem { Header = "👁️ Просмотр" };
                viewItem.Click += (s, args) => ViewPhoto(photo);

                var shareItem = new MenuItem { Header = "🔗 Поделиться" };
                shareItem.Click += (s, args) => SharePhoto(photo);

                var exportItem = new MenuItem { Header = "📤 Экспорт" };
                exportItem.Click += (s, args) => ExportPhoto(photo);

                var deleteItem = new MenuItem { Header = "🗑️ Удалить" };
                deleteItem.Click += (s, args) => DeletePhoto(photo);

                contextMenu.Items.Add(viewItem);
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(shareItem);
                contextMenu.Items.Add(exportItem);
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(deleteItem);

                contextMenu.IsOpen = true;
            }
        }

        private void ViewPhoto(Photo photo)
        {
            // Здесь можно открыть окно просмотра фото в полном размере
            MessageBox.Show(
                $"Просмотр: {photo.OriginalName}\nРазмер: {photo.Dimensions}\nВес: {photo.SizeDisplay}",
                "Просмотр фото",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void SharePhoto(Photo photo)
        {
            ShareDialog.ShowShareDialog(photo);
        }

        private void ExportPhoto(Photo photo)
        {
            var exportWindow = new ExportWindow();
            exportWindow.Owner = this;
            exportWindow.ShowDialog();
        }

        private void DeletePhoto(Photo photo)
        {
            _viewModel?.DeletePhotoCommand.Execute(photo);
        }

        private void FolderContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Folder folder)
            {
                var contextMenu = new ContextMenu();

                var openItem = new MenuItem { Header = "📂 Открыть" };
                openItem.Click += (s, args) => _viewModel?.NavigateToFolderCommand.Execute(folder);

                var renameItem = new MenuItem { Header = "✏️ Переименовать" };
                renameItem.Click += (s, args) => _viewModel?.RenameFolderCommand.Execute(folder);

                var shareItem = new MenuItem { Header = "🔗 Поделиться" };
                shareItem.Click += (s, args) => ShareDialog.ShowShareDialog(folder);

                var deleteItem = new MenuItem { Header = "🗑️ Удалить" };
                deleteItem.Click += (s, args) => _viewModel?.DeleteFolderCommand.Execute(folder);

                contextMenu.Items.Add(openItem);
                contextMenu.Items.Add(renameItem);
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(shareItem);
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(deleteItem);

                contextMenu.IsOpen = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Горячие клавиши
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        _viewModel?.CreateFolderCommand.Execute(null);
                        break;
                    case Key.I:
                        _viewModel?.OpenImportWindow();
                        break;
                    case Key.E:
                        _viewModel?.OpenExportWindow();
                        break;
                    case Key.A:
                        _viewModel?.SelectAllPhotos();
                        break;
                    case Key.F5:
                        _viewModel?.RefreshCommand.Execute(null);
                        break;
                    case Key.Delete:
                        _viewModel?.DeleteSelectedPhotosCommand.Execute(null);
                        break;
                }
            }
        }
    }
}