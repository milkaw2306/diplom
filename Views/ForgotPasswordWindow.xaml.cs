using System.Windows;

namespace Diplom_zxc.Views
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();

            NewPasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is ViewModels.ForgotPasswordViewModel viewModel)
                {
                    viewModel.NewPassword = NewPasswordBox.Password;
                }
            };
        }
    }
}