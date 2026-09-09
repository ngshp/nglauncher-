using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace NgpbLauncher.Services
{
    /// <summary>
    /// SecurityGuard: Menangani HWID Generation dan IP Validation.
    /// Bertanggung jawab mengunci launcher ke hardware spesifik pengguna.
    /// </summary>
    public static class SecurityGuard
    {
        // Ganti dengan Secret Key unik aplikasi Anda (minimal 32 karakter)
        private const string APP_SECRET_KEY = "NGPB-ENTERPRISE-SECURITY-KEY-2026-V1";

        public static bool IsHardwareAllowed()
        {
            try
            {
                // 1. Generate HWID dari PC saat ini
                string currentHwid = GenerateUniqueHardwareId();
                
                // 2. Cek ke Server/Registry apakah HWID ini terdaftar
                // Untuk demo ini, kita simpan di Registry lokal. 
                // Di production, ganti dengan API call ke server NGPB.
                return IsHwidRegistered(currentHwid);
            }
            catch
            {
                return false;
            }
        }

        public static string GenerateUniqueHardwareId()
        {
            var sb = new StringBuilder();
            
            try
            {
                // Ambil CPU ID
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        sb.Append(obj["ProcessorId"]?.ToString());
                        break; // Ambil CPU pertama saja
                    }
                }

                // Ambil Motherboard Serial Number
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        sb.Append(obj["SerialNumber"]?.ToString());
                        break;
                    }
                }

                // Ambil MAC Address Adapter Utama
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up && 
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var mac = nic.GetPhysicalAddress().ToString();
                        if (!string.IsNullOrEmpty(mac))
                        {
                            sb.Append(mac);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback jika WMI gagal diakses
                sb.Append(Environment.MachineName + Environment.UserName);
            }

            // Hash hasil gabungan agar tidak mudah dibaca/dipalsukan
            return ComputeSha256Hash(sb.ToString() + APP_SECRET_KEY);
        }

        private static bool IsHwidRegistered(string hwid)
        {
            // LOGIKA PRODUCTION:
            // Di sini Anda harus memanggil API Server NGPB untuk cek:
            // var response = await HttpClient.GetAsync($"https://api.ngpb.com/check-hwid?hwid={hwid}");
            
            // LOGIKA DEMO/LOKAL:
            // Kita anggap semua HWID yang valid (tidak kosong) diizinkan untuk testing.
            // Nanti Anda bisa tambahkan logic whitelist di sini.
            return !string.IsNullOrEmpty(hwid) && hwid.Length > 10;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
