using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;

namespace NgpbLauncher.Services
{
    /// <summary>
    /// SecurityBootManager: Menangani Layer 1 (Integrity) dan Layer 2 (Environment) Validation.
    /// Bertanggung jawab atas deteksi bypass dan manajemen status banned.
    /// </summary>
    public static class SecurityBootManager
    {
        // Ganti dengan SHA-256 Hash asli dari NgpbLauncher.exe setelah di-build
        private const string EXPECTED_HASH = "YOUR_EXPECTED_HASH_HERE"; 
        
        private const string BAN_KEY_PATH = @"SOFTWARE\NGPB\Security\BanData";
        
        // Event untuk mengirim progress ke UI (SecureBootWindow)
        public static event Action<string, int> OnLayerProgress;
        
        // Event untuk memberitahu UI jika terjadi pelanggaran
        public static event Action<string> OnViolationDetected;

        public static async Task<bool> ExecuteSecureBootAsync()
        {
            try
            {
                // --- LAYER 1: INTEGRITY CHECK ---
                Report("Verifying Cryptographic Hashes...", 20);
                await Task.Delay(800); // Simulasi proses berat
                
                if (!VerifyExecutableHash())
                {
                    TriggerViolation("File integrity compromised. Modified executable detected.");
                    return false;
                }

                // --- LAYER 2: ENVIRONMENT CHECK ---
                Report("Scanning for Debuggers & Injectors...", 50);
                await Task.Delay(800);

                if (NativeProtection.IsDebuggerAttached() || HasSuspiciousProcesses())
                {
                    TriggerViolation("Unauthorized debugging tool or injector detected.");
                    return false;
                }

                // --- LAYER 3: NETWORK & HWID BINDING ---
                Report("Validating Hardware ID Binding...", 80);
                await Task.Delay(800);

                if (!SecurityGuard.IsHardwareAllowed())
                {
                    TriggerViolation("Hardware ID mismatch. Access denied.");
                    return false;
                }

                Report("Security Validation Complete.", 100);
                await Task.Delay(500);
                return true;
            }
            catch (Exception ex)
            {
                TriggerViolation($"Critical Security Error: {ex.Message}");
                return false;
            }
        }

        private static bool VerifyExecutableHash()
        {
            try
            {
                var path = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(path)) return false;

                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(path);
                var hashBytes = sha256.ComputeHash(stream);
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                // Jika hash belum diset (masih development), kita bypass sementara
                if (EXPECTED_HASH == "YOUR_EXPECTED_HASH_HERE") return true;

                return hashString == EXPECTED_HASH;
            }
            catch
            {
                return false;
            }
        }

        private static bool HasSuspiciousProcesses()
        {
            // Daftar nama proses yang umum dipakai untuk cheat/debugging
            string[] suspiciousNames = { "cheatengine", "x64dbg", "ollydbg", "injector", "memory_hack" };
            
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    var name = proc.ProcessName.ToLowerInvariant();
                    foreach (var suspicious in suspiciousNames)
                    {
                        if (name.Contains(suspicious)) return true;
                    }
                }
                catch { /* Ignore access denied processes */ }
            }
            return false;
        }

        private static void TriggerViolation(string reason)
        {
            // Catat pelanggaran di Registry (untuk Permanent Ban logic)
            RecordViolation(reason);
            
            // Kirim sinyal ke UI untuk menampilkan overlay merah
            OnViolationDetected?.Invoke(reason);
        }

        private static void RecordViolation(string reason)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(BAN_KEY_PATH);
                int violationCount = (int)(key.GetValue("ViolationCount", 0) ?? 0);
                violationCount++;
                
                key.SetValue("ViolationCount", violationCount);
                key.SetValue("LastReason", reason);
                key.SetValue("LastDate", DateTime.Now.ToString());

                // Jika sudah 3x pelanggaran, set status Banned Permanen
                if (violationCount >= 3)
                {
                    key.SetValue("IsBanned", 1);
                }
            }
            catch { /* Ignore registry errors */ }
        }

        public static bool IsPermanentlyBanned()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(BAN_KEY_PATH);
                return key?.GetValue("IsBanned", 0) as int? == 1;
            }
            catch
            {
                return false;
            }
        }

        private static void Report(string message, int percentage)
        {
            OnLayerProgress?.Invoke(message, percentage);
        }
    }
}
