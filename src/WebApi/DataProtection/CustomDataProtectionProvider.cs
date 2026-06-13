using Microsoft.Owin.Security.DataProtection;

namespace SdtdServerKit.WebApi.DataProtection
{
    /// <summary>
    /// Implement IDataProtectionProvider
    /// </summary>
    internal class CustomDataProtectionProvider : IDataProtectionProvider
    {
        private static readonly IDataProtector _dataProtector = new AesDataProtector();

        public static IDataProtector DataProtector => _dataProtector;

        public IDataProtector Create(params string[] purposes)
        {
            return _dataProtector;
        }
    }
}