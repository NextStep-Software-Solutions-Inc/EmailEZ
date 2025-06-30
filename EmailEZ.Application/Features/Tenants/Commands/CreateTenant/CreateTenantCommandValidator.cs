using FluentValidation;
using System;

namespace EmailEZ.Application.Features.Tenants.Commands.CreateTenant;

/// <summary>
/// Validator for the CreateTenantCommand.
/// </summary>
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(255).WithMessage("Tenant name must not exceed 255 characters.");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Domain is required.")
            .MaximumLength(255).WithMessage("Domain must not exceed 255 characters.")
            .Must(BeAValidDomain).WithMessage("Please provide a valid domain format.");
    }

    private bool BeAValidDomain(string domain)
    {
        // Simple regex for domain validation. For a production app, consider a more robust library
        // or a dedicated service for domain validation.
        // This regex ensures it's not empty, contains allowed characters, and has at least one dot.
        return !string.IsNullOrWhiteSpace(domain) &&
               System.Text.RegularExpressions.Regex.IsMatch(domain, @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,6}$");
    }
}