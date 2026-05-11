using System.Windows;

namespace Diplom_zxc.Views
{
    /// <summary>
    /// Логика взаимодействия для ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        public ImportWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}