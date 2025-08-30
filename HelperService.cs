using Microsoft.AspNetCore.DataProtection;
namespace FreedomITAS
{
    public class RouteProtector
    {
        private readonly IDataProtector _protector;

        public RouteProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("RouteValueProtector");
        }

        public string Protect(string input)
        {
            return _protector.Protect(input);
        }

        public string Unprotect(string input)
        {
            return _protector.Unprotect(input);
        }
    }

}
