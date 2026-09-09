using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NgpbLauncher.Services
{
    /// <summary>
    /// NativeProtection: Menangani proteksi tingkat rendah menggunakan Windows API.
    /// Bertanggung jawab atas Anti-Debug, DEP, dan Process Hardening.
    /// </summary>
    public static class NativeProtection
    {
        // --- WINDOWS API IMPORTS ---

        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll")]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessDEPPolicy(uint dwFlags);

        [DllImport("kernel32.dll")]
        private static extern bool SetHandleInformation(IntPtr hObject, uint dwMask, uint dwFlags);

        // Konstanta untuk DEP (Data Execution Prevention)
        private const uint PROCESS_DEP_ENABLE = 0x00000001;
        
        // Konstanta untuk Handle Protection
        private const uint HANDLE_FLAG_PROTECT_FROM_CLOSE = 0x00000002;

        /// <summary>
        /// Menerapkan semua proteksi native saat aplikasi dimulai.
        /// </summary>
        public static void ApplyNativeProtections()
        {
            try
            {
                // 1. Cek Debugger Sederhana
                if (IsDebuggerPresent())
                {
                    Environment.FailFast("Debugger detected. Access denied.");
                }

                // 2. Cek Remote Debugger (lebih advanced)
                bool isRemoteDebugged = false;
                CheckRemoteDebuggerPresent(GetCurrentProcess(), ref isRemoteDebugged);
                if (isRemoteDebugged)
                {
                    Environment.FailFast("Remote debugging detected. Access denied.");
                }

                // 3. Aktifkan DEP (Mencegah eksekusi kode di area memori data)
                // Ini membuat cheat berbasis code-injection lebih sulit bekerja
                SetProcessDEPPolicy(PROCESS_DEP_ENABLE);

                // 4. Proteksi Handle Proses
                // Mencegah tool seperti Process Hacker menutup paksa launcher
                var handle = GetCurrentProcess();
                SetHandleInformation(handle, HANDLE_FLAG_PROTECT_FROM_CLOSE, HANDLE_FLAG_PROTECT_FROM_CLOSE);
            }
            catch
            {
                // Jika gagal menerapkan proteksi (misal karena OS lama), 
                // kita tetap izinkan jalan tapi dengan risiko lebih tinggi.
                // Di production, sebaiknya tetap FailFast.
            }
        }

        /// <summary>
        /// Helper method untuk mengecek status debugger secara real-time.
        /// Bisa dipanggil berkala di background thread.
        /// </summary>
        public static bool IsDebuggerAttached()
        {
            return IsDebuggerPresent();
        }
    }
}
