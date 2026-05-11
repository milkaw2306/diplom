using System.Windows;
using Diplom_zxc.ViewModels;

namespace Diplom_zxc.Views
{
    /// <summary>
    /// Логика взаимодействия для ForgotPasswordWindow.xaml
    /// </summary>
    public partial class ForgotPasswordWindow : Window
    {
        private readonly ForgotPasswordViewModel? _viewModel;

        public ForgotPasswordWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as ForgotPasswordViewModel;

            // Привязываем PasswordBox к ViewModel
            NewPasswordBox.PasswordChanged += (s, e) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.NewPassword = NewPasswordBox.Password;
                }
            };
        }
    }
}