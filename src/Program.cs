using System;
using System.Windows;
using NgpbLauncher.Services;
using NgpbLauncher.Views;

namespace NgpbLauncher
{
    /// <summary>
    /// Entry Point Aplikasi NGPB Launcher
    /// Bertanggung jawab atas inisialisasi keamanan sebelum UI dimuat.
    /// </summary>
    public static class Program
    {
        [STAThread]
        // WAJIB tambahkan string[] args agar kompatibel dengan PublishTrimmed + SelfContained
        public static void Main(string[] args) 
        {
            try
            {
                // LAYER 0: Native Protection (Anti-Debug & DEP)
                NativeProtection.ApplyNativeProtections();

                // Cek Status Ban Permanen (HWID & IP)
                if (SecurityBootManager.IsPermanentlyBanned())
                {
                    MessageBox.Show(
                        "⛔ PERMANENTLY BANNED ⛔\n\n" +
                        "Hardware ID dan IP Address Anda telah diblokir secara permanen dari NGPB.\n" +
                        "Tidak ada banding yang dapat diterima.",
                        "NGPB - SECURITY ALERT", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Stop);
                    
                    Environment.Exit(1);
                }

                // Inisialisasi Aplikasi WPF dengan App.xaml
                // Menggunakan App() memastikan resource dictionary & global handlers terbaca
                var app = new App(); 
                app.InitializeComponent(); 
                
                // Menjalankan SecureBootWindow sebagai jendela pertama
                app.Run(new SecureBootWindow());
            }
            catch (Exception ex)
            {
                // Handle error kritis saat startup
                MessageBox.Show(
                    $"FATAL ERROR: {ex.Message}\n\nLauncher tidak dapat memulai sistem keamanan.",
                    "NGPB - CRITICAL FAILURE",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }
    }
}
