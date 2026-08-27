using CivicFix.Application.Common.Security;
using FluentAssertions;
using Xunit;

namespace CivicFix.Tests.Unit;

public class PiiRedactionServiceTests
{
    private readonly IPiiRedactionService _piiService;

    public PiiRedactionServiceTests()
    {
        _piiService = new PiiRedactionService();
    }

    [Fact]
    public void RedactPii_ShouldMaskIndianPhoneNumbers()
    {
        var input = "Please call citizen Ramesh at +91-9876543210 or 9876543210 regarding water leak.";
        var redacted = _piiService.RedactPii(input);

        redacted.Should().NotContain("9876543210");
        redacted.Should().Contain("[PHONE REDACTED]");
    }

    [Fact]
    public void RedactPii_ShouldMaskIndianAadhaarNumbers()
    {
        var input = "Resident Aadhaar verified: 2345 6789 0123 for municipal grievance registration.";
        var redacted = _piiService.RedactPii(input);

        redacted.Should().NotContain("2345 6789 0123");
        redacted.Should().Contain("[AADHAAR REDACTED]");
    }

    [Fact]
    public void RedactPii_ShouldMaskEmailAddresses()
    {
        var input = "Contact resident at citizen.sonipat@gmail.com for gate key.";
        var redacted = _piiService.RedactPii(input);

        redacted.Should().NotContain("citizen.sonipat@gmail.com");
        redacted.Should().Contain("[EMAIL REDACTED]");
    }

    [Fact]
    public void ContainsSensitiveInformation_ShouldReturnTrue_WhenPiiIsPresent()
    {
        var textWithPhone = "Emergency contact: +91-9876540001";
        var textWithoutPii = "Broken street light pole in Sector 14 near community center.";

        _piiService.ContainsSensitiveInformation(textWithPhone).Should().BeTrue();
        _piiService.ContainsSensitiveInformation(textWithoutPii).Should().BeFalse();
    }
}
