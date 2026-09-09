using System;
using System.Windows;
using NgpbLauncher.Services;
using NgpbLauncher.Views;

namespace NgpbLauncher
{
    public static class Program
    {
        [STAThread]
        // ⚠️ WAJIB ADA PARAMETER string[] args UNTUK TRIMMING + SELF-CONTAINED
        public static void Main(string[] args) 
        {
            try
            {
                // LAYER 0: Native Protection
                // Jika trimming gagal karena ini, kita butuh trimmer config khusus
                NativeProtection.ApplyNativeProtections();

                if (SecurityBootManager.IsPermanentlyBanned())
                {
                    MessageBox.Show(
                        "⛔ PERMANENTLY BANNED ⛔\n\n" +
                        "Hardware ID dan IP Address Anda telah diblokir secara permanen.",
                        "NGPB - SECURITY ALERT", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Stop);
                    Environment.Exit(1);
                }

                var app = new App();
                app.InitializeComponent();
                app.Run(new SecureBootWindow());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"FATAL ERROR: {ex.Message}",
                    "NGPB - CRITICAL FAILURE",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }
    }
}
