using System;
using System.Windows;
using NgpbLauncher.Services;

namespace NgpbLauncher.Views
{
    /// <summary>
    /// Interaction logic for SecureBootWindow.xaml
    /// Window ini menangani Layer 1 & 2 Security Validation sebelum masuk ke Main Launcher.
    /// </summary>
    public partial class SecureBootWindow : Window
    {
        public SecureBootWindow()
        {
            InitializeComponent();

            // Mendaftarkan Event Handler untuk menerima update progress dari SecurityBootManager
            SecurityBootManager.OnLayerProgress += (message, percentage) => 
            {
                // Karena event ini datang dari thread background, kita harus pakai Dispatcher
                Dispatcher.Invoke(() => 
                {
                    StatusText.Text = message;
                    SecurityProgressBar.Value = percentage;
                });
            };

            // Mendaftarkan Event Handler jika terjadi pelanggaran keamanan
            SecurityBootManager.OnViolationDetected += (reason) => 
            {
                Dispatcher.Invoke(() => 
                {
                    ViolationReasonText.Text = reason;
                    ViolationOverlay.Visibility = Visibility.Visible;
                });
            };

            // Memulai proses booting saat window selesai dimuat
            Loaded += async (_, _) => 
            {
                bool isSecure = await SecurityBootManager.ExecuteSecureBootAsync();

                if (isSecure)
                {
                    // Jika aman, tutup loading screen dan buka MainWindow
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    // Jika gagal (seharusnya sudah ditangani oleh OnViolationDetected),
                    // kita pastikan aplikasi tetap tidak masuk ke main menu.
                    // Biarkan user melihat pesan error di overlay.
                }
            };
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
