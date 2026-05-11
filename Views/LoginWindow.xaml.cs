using System.Windows;
using System.Windows.Controls;
using Diplom_zxc.ViewModels;

namespace Diplom_zxc.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel? _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as LoginViewModel;

            // Привязываем PasswordBox к ViewModel
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.Password = PasswordBox.Password;
                }
            };
        }
    }
}