using System.Windows;

namespace NgpbLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Mencegah crash jika ada thread yang error di background
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Log error atau tampilkan pesan jika terjadi exception yang tidak tertangkap
            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}",
                "NGPB Launcher Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Tandai bahwa error sudah ditangani agar aplikasi tidak langsung close paksa
            e.Handled = true;
        }
    }
}
