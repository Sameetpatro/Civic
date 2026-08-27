using System.Text.RegularExpressions;

namespace CivicFix.Application.Common.Security;

public class PiiRedactionService : IPiiRedactionService
{
    // Indian Phone Number Pattern (+91 or standard 10 digits starting with 6-9)
    private static readonly Regex PhoneRegex = new(
        @"(?:\+91[\-\s]?)?[6-9]\d{9}|\b[6-9]\d{4}[\-\s]\d{5}\b", 
        RegexOptions.Compiled);

    // Indian Aadhaar Card Pattern (12 digits with optional spaces or hyphens)
    private static readonly Regex AadhaarRegex = new(
        @"\b[2-9]\d{3}[\s\-]?\d{4}[\s\-]?\d{4}\b", 
        RegexOptions.Compiled);

    // Email Pattern
    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+", 
        RegexOptions.Compiled);

    public string RedactPii(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var result = EmailRegex.Replace(input, "[EMAIL REDACTED]");
        result = AadhaarRegex.Replace(result, "[AADHAAR REDACTED]");
        result = PhoneRegex.Replace(result, "[PHONE REDACTED]");

        return result;
    }

    public bool ContainsSensitiveInformation(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return PhoneRegex.IsMatch(input) || 
               AadhaarRegex.IsMatch(input) || 
               EmailRegex.IsMatch(input);
    }
}
