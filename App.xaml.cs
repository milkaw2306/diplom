using System.Windows;

namespace Diplom_zxc
{
    public partial class App : Application
    {
        public static int CurrentUserId { get; set; }
        public static string? CurrentUsername { get; set; }
        public static string ConnectionString { get; } =
            "server=localhost;port=3306;database=diplom_zxc;uid=root;password=your_password;";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Инициализация базы данных
            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            try
            {
                var dbService = new Services.DatabaseService(ConnectionString);
                await dbService.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}\n\n" +
                              "Проверьте, запущен ли MySQL сервер и правильно ли указаны настройки подключения.",
                              "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);

                Current.Shutdown();
            }
        }
    }
}