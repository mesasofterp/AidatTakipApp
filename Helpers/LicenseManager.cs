using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace StudentApp.Helpers
{
    public class License
    {
        public string MachineHash { get; set; }
        public DateTime Expiry { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Product { get; set; }
    }

    public static class LicenseManager
    {
        public static bool CheckLicense(string publicKeyPath, string licensePath, out string reason)
        {
            reason = "";
            if (!File.Exists(licensePath))
            {
                reason = "Lisans dosyası bulunamadı.";
                return false;
            }

            string package = File.ReadAllText(licensePath);
            var parts = package.Split('.');
            if (parts.Length != 2)
            {
                reason = "Lisans formatı geçersiz.";
                return false;
            }

            byte[] jsonBytes = Convert.FromBase64String(parts[0]);
            byte[] sig = Convert.FromBase64String(parts[1]);

            if (!File.Exists(publicKeyPath))
            {
                reason = "Public key bulunamadı.";
                return false;
            }

            try
            {
                byte[] pubKeyBytes;
                string pem = File.ReadAllText(publicKeyPath).Trim();
                if (pem.StartsWith("-----BEGIN PUBLIC KEY-----"))
                {
                    // PEM -> DER (SubjectPublicKeyInfo)
                    string base64 = pem
                        .Replace("-----BEGIN PUBLIC KEY-----", "")
                        .Replace("-----END PUBLIC KEY-----", "")
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Trim();
                    pubKeyBytes = Convert.FromBase64String(base64);
                }
                else
                {
                    // eski DER format
                    pubKeyBytes = File.ReadAllBytes(publicKeyPath);
                }

                using var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(pubKeyBytes, out _); // PEM ile uyumlu

                bool valid = rsa.VerifyData(jsonBytes, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                if (!valid)
                {
                    reason = "Lisans imzası geçersiz.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                reason = "Public key okunamadı veya geçersiz: " + ex.Message;
                return false;
            }

            string json = Encoding.UTF8.GetString(jsonBytes);
            License? lic;
            try
            {
                lic = JsonSerializer.Deserialize<License>(json);
            }
            catch
            {
                reason = "Lisans dosyası okunamadı (JSON hatası).";
                return false;
            }

            if (lic == null)
            {
                reason = "Lisans geçersiz.";
                return false;
            }

            string current = MachineIdHelper.GetMachineIdHash();
            if (lic.MachineHash != current)
            {
                reason = "Lisans bu makineye ait değil.";
                return false;
            }

            if (DateTime.UtcNow > lic.Expiry)
            {
                reason = $"Lisans süresi dolmuş ({lic.Expiry:yyyy-MM-dd}).";
                return false;
            }

            return true;
        }
    }
}
