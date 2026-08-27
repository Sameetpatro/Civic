namespace CivicFix.Application.Common.Security;

public interface IPiiRedactionService
{
    string RedactPii(string? input);
    bool ContainsSensitiveInformation(string? input);
}
