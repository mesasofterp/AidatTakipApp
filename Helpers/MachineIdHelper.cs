using System.Management;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;
namespace StudentApp.Helpers
{
    public static class MachineIdHelper
    {
        public static string GetMachineIdHash()
        {
            string machineGuid = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography",
                "MachineGuid",
                ""
            )?.ToString() ?? "";

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(machineGuid));
            return Convert.ToBase64String(hash);
        }

        static string GetWmi(string wmiClass, string prop)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT {prop} FROM {wmiClass}");
                foreach (var mo in searcher.Get())
                {
                    var v = mo[prop];
                    if (v != null) return v.ToString();
                }
            }
            catch { }
            return "";
        }
    }
}
