using System.Net;

namespace TestPilot.Application.Interfaces;

public interface ISsrfProtectionService
{
    Task ValidateUrlAsync(string url, bool allowLocalhost = false);
    bool IsPrivateOrRestrictedIp(IPAddress ipAddress);
}
