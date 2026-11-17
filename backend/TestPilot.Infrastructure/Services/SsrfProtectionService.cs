using System.Net;
using System.Net.Sockets;
using TestPilot.Application.Interfaces;

namespace TestPilot.Infrastructure.Services;

public class SsrfProtectionService : ISsrfProtectionService
{
    public async Task ValidateUrlAsync(string url, bool allowLocalhost = false)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("The request URL cannot be empty.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid URL format: '{url}'. Please include scheme (http:// or https://).", nameof(url));
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException($"Unsupported URL scheme '{uri.Scheme}'. Only HTTP and HTTPS are permitted.", nameof(url));
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new ArgumentException("Userinfo (username:password@host) is not permitted in request URLs.", nameof(url));
        }

        if (allowLocalhost)
        {
            return;
        }

        var host = uri.DnsSafeHost;

        // Fast-path for localhost string check
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("127.0.0.1") ||
            host.Equals("::1"))
        {
            throw new InvalidOperationException("Requests to loopback / localhost addresses are blocked for security.");
        }

        // Fast-path if host is direct IP address
        if (IPAddress.TryParse(host, out var directIp))
        {
            if (IsPrivateOrRestrictedIp(directIp))
            {
                throw new InvalidOperationException($"Requests to restricted IP address '{directIp}' are blocked.");
            }
            return;
        }

        // Resolve DNS and verify all returned addresses
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host);
            if (addresses.Length == 0)
            {
                throw new InvalidOperationException($"Unable to resolve host '{host}'.");
            }

            foreach (var ip in addresses)
            {
                if (IsPrivateOrRestrictedIp(ip))
                {
                    throw new InvalidOperationException(
                        $"Host '{host}' resolved to restricted IP address '{ip}'. Execution blocked by SSRF protection.");
                }
            }
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException($"DNS resolution failed for host '{host}': {ex.Message}", ex);
        }
    }

    public bool IsPrivateOrRestrictedIp(IPAddress ipAddress)
    {
        if (IPAddress.IsLoopback(ipAddress))
            return true;

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6Multicast || ipAddress.IsIPv6SiteLocal)
                return true;

            var bytes = ipAddress.GetAddressBytes();
            // Unique Local Addresses fc00::/7 (fc00... and fd00...)
            if ((bytes[0] & 0xFE) == 0xFC)
                return true;

            return false;
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ipAddress.GetAddressBytes();

            // 0.0.0.0/8 (Current network)
            if (bytes[0] == 0)
                return true;

            // 10.0.0.0/8 (Private network)
            if (bytes[0] == 10)
                return true;

            // 127.0.0.0/8 (Loopback)
            if (bytes[0] == 127)
                return true;

            // 169.254.0.0/16 (Link-local / APIPA / Cloud metadata 169.254.169.254)
            if (bytes[0] == 169 && bytes[1] == 254)
                return true;

            // 172.16.0.0/12 (Private network: 172.16.0.0 - 172.31.255.255)
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0/16 (Private network)
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;

            // 224.0.0.0/4 (Multicast: 224.0.0.0 - 239.255.255.255)
            if (bytes[0] >= 224 && bytes[0] <= 239)
                return true;

            // 240.0.0.0/4 (Reserved)
            if (bytes[0] >= 240)
                return true;

            // 255.255.255.255 (Broadcast)
            if (bytes[0] == 255 && bytes[1] == 255 && bytes[2] == 255 && bytes[3] == 255)
                return true;
        }

        return false;
    }
}
