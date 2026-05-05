using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Diplom_zxc.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is ViewModels.LoginViewModel viewModel)
                {
                    viewModel.Password = PasswordBox.Password;
                }
            };
        }
    }

    public static class InverseBoolConverter
    {
        public static readonly IValueConverter Instance = new InvertBoolConverter();

        private class InvertBoolConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value is bool b && !b;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value is bool b && !b;
            }
        }
    }
}