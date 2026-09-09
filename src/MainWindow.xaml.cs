using System;
using System.Windows;
using System.Windows.Input;
using NgpbLauncher.Services;

namespace NgpbLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AutoUpdateManager _updateManager;

        public MainWindow()
        {
            InitializeComponent();
            
            // Inisialisasi Manager Update
            _updateManager = new AutoUpdateManager();
            
            // Setup Event Handler untuk System Tray
            NotifyIcon.TrayLeftMouseDown += (s, e) => this.Show();
            NotifyIcon.TrayRightMouseDown += (s, e) => NotifyIcon.ContextMenu.IsOpen = true;

            // Mulai proses validasi dan update saat window dimuat
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus.Text = "Connecting to NGPB Server...";
                
                // STEP 1: Cek Update & Repair File
                var result = await _updateManager.CheckAndApplyUpdatesAsync();
                
                if (!result.IsSuccess && !result.RestartRequired)
                {
                    UpdateStatus.Text = "Update Failed: " + result.Message;
                    UpdateStatus.Foreground = System.Windows.Media.Brushes.Red;
                    PlayButton.IsEnabled = false;
                    return;
                }

                if (result.RestartRequired)
                {
                    MessageBox.Show("Launcher has been updated. Please restart.", "NGPB Update", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Shutdown();
                    return;
                }

                // STEP 2: Validasi Lingkungan (Post-Update Check)
                UpdateStatus.Text = "Validating Environment...";
                // Di sini bisa ditambahkan cek SecurityGuard lagi jika perlu
                
                // STEP 3: Launcher Siap
                UpdateStatus.Text = "Ready to Launch";
                UpdateStatus.Foreground = System.Windows.Media.Brushes.Green;
                PlayButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                UpdateStatus.Text = "Error: " + ex.Message;
                PlayButton.IsEnabled = false;
            }
        }

        // --- LOGIKA TOMBOL UI ---

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // Logika menjalankan game.exe akan ditaruh di sini nanti
            MessageBox.Show("Launching Point Blank Game Client...", "NGPB Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Contoh: Process.Start("PointBlank.exe");
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Sembunyikan ke System Tray alih-alih menutup total
            this.Hide();
            NotifyIcon.ShowBalloonTip("NGPB Launcher", "Launcher is minimized to tray.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }

        // --- LOGIKA DRAG WINDOW (Karena WindowStyle=None) ---
        
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
