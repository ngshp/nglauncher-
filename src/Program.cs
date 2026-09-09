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
        public static void Main()
        {
            try
            {
                // LAYER 0: Native Protection (Anti-Debug & DEP)
                // Dijalankan paling awal sebelum WPF diinisialisasi
                NativeProtection.ApplyNativeProtections();

                // Cek Status Ban Permanen (HWID & IP)
                // Jika terdeteksi banned, aplikasi langsung mati tanpa menampilkan UI
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

                // Inisialisasi Aplikasi WPF
                var app = new Application();
                
                // Menjalankan SecureBootWindow sebagai jendela pertama (bukan MainWindow)
                // Ini memastikan Layer 1 & 2 validasi keamanan berjalan dulu
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
