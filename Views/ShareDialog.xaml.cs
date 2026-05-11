using System.Windows;
using Diplom_zxc.Models;
using Diplom_zxc.ViewModels;

namespace Diplom_zxc.Views
{
    /// <summary>
    /// Логика взаимодействия для ShareDialog.xaml
    /// Окно для создания и управления ссылками для шаринга
    /// </summary>
    public partial class ShareDialog : Window
    {
        private readonly ShareViewModel? _viewModel;

        public ShareDialog()
        {
            InitializeComponent();
            _viewModel = DataContext as ShareViewModel;
        }

        /// <summary>
        /// Конструктор для шаринга папки
        /// </summary>
        public ShareDialog(Folder folder) : this()
        {
            if (_viewModel != null)
            {
                _viewModel.InitializeWithFolder(folder);
            }
        }

        /// <summary>
        /// Конструктор для шаринга фотографии
        /// </summary>
        public ShareDialog(Photo photo) : this()
        {
            if (_viewModel != null)
            {
                _viewModel.InitializeWithPhoto(photo);
            }
        }

        /// <summary>
        /// Показать диалог шаринга для папки
        /// </summary>
        public static void ShowShareDialog(Folder folder)
        {
            var dialog = new ShareDialog(folder);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        /// <summary>
        /// Показать диалог шаринга для фотографии
        /// </summary>
        public static void ShowShareDialog(Photo photo)
        {
            var dialog = new ShareDialog(photo);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        /// <summary>
        /// Обработчик закрытия окна
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}