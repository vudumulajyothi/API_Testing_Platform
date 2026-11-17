using System.Net;
using FluentAssertions;
using TestPilot.Infrastructure.Services;
using Xunit;

namespace TestPilot.Tests.Unit;

public class SsrfProtectionServiceTests
{
    private readonly SsrfProtectionService _ssrfService = new();

    [Theory]
    [InlineData("http://localhost:5000/api")]
    [InlineData("https://localhost")]
    [InlineData("http://127.0.0.1:8080")]
    [InlineData("http://[::1]")]
    public async Task ValidateUrlAsync_LoopbackAddress_ShouldThrowInvalidOperationException(string url)
    {
        // Act
        var act = async () => await _ssrfService.ValidateUrlAsync(url, allowLocalhost: false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*blocked*");
    }

    [Theory]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("http://10.0.0.1/admin")]
    [InlineData("http://172.16.0.10:3000")]
    [InlineData("http://192.168.1.254/config")]
    public async Task ValidateUrlAsync_PrivateAndCloudMetadataIps_ShouldThrowInvalidOperationException(string url)
    {
        // Act
        var act = async () => await _ssrfService.ValidateUrlAsync(url, allowLocalhost: false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*restricted IP*");
    }

    [Theory]
    [InlineData("ftp://example.com/file.txt")]
    [InlineData("file:///etc/passwd")]
    [InlineData("gopher://example.com")]
    public async Task ValidateUrlAsync_NonHttpSchemes_ShouldThrowArgumentException(string url)
    {
        // Act
        var act = async () => await _ssrfService.ValidateUrlAsync(url, allowLocalhost: false);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Only HTTP and HTTPS are permitted*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-valid-url")]
    public async Task ValidateUrlAsync_MalformedUrls_ShouldThrowArgumentException(string url)
    {
        // Act
        var act = async () => await _ssrfService.ValidateUrlAsync(url, allowLocalhost: false);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void IsPrivateOrRestrictedIp_ShouldIdentifyRestrictedRangesAccurately()
    {
        // Loopback
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("127.0.0.1")).Should().BeTrue();
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("::1")).Should().BeTrue();

        // AWS/Azure metadata
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("169.254.169.254")).Should().BeTrue();

        // RFC 1918 Private networks
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("10.10.10.10")).Should().BeTrue();
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("172.16.5.5")).Should().BeTrue();
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("172.31.255.254")).Should().BeTrue();
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("192.168.0.1")).Should().BeTrue();

        // Public IPs (should return false)
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("8.8.8.8")).Should().BeFalse();
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("1.1.1.1")).Should().BeFalse();
        _ssrfService.IsPrivateOrRestrictedIp(IPAddress.Parse("93.184.216.34")).Should().BeFalse();
    }
}
