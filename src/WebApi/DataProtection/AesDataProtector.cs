using DeviceId;
using IceCoffee.Common.Security.Cryptography;
using Microsoft.Owin.Security.DataProtection;
using System.Text;

namespace SdtdServerKit.WebApi.DataProtection
{
    /// <summary>
    /// IDataProtector
    /// </summary>
    internal class AesDataProtector : IDataProtector
    {
        // len: 16
        private static readonly byte[] _key;
        private static readonly byte[] _iv;

        static AesDataProtector()
        {
            var currentDeviceId = new DeviceIdBuilder()
                .AddMachineName()
                .AddUserName()
                .AddMacAddress()
                .AddOsVersion()
                .ToString();

            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            _key = md5.ComputeHash(Encoding.UTF8.GetBytes(currentDeviceId));
            _iv = md5.ComputeHash(Encoding.UTF8.GetBytes(UnityEngine.Device.SystemInfo.deviceUniqueIdentifier));
        }

        public byte[] Protect(byte[] plaintext)
        {
            return AES.Encrypt(plaintext, _key, _iv);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return AES.Decrypt(protectedData, _key, _iv);
        }
    }
}