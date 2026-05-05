using System.Windows;

namespace Diplom_zxc.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is ViewModels.RegisterViewModel viewModel)
                {
                    viewModel.Password = PasswordBox.Password;
                }
            };

            ConfirmPasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is ViewModels.RegisterViewModel viewModel)
                {
                    viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
                }
            };
        }
    }
}