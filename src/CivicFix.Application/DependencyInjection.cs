using CivicFix.Application.Features.Auth;
using CivicFix.Application.Features.Departments;
using CivicFix.Application.Features.Issues;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CivicFix.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IIssueService, IssueService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddSingleton<CivicFix.Application.Common.Security.IPiiRedactionService, CivicFix.Application.Common.Security.PiiRedactionService>();

        return services;
    }
}
