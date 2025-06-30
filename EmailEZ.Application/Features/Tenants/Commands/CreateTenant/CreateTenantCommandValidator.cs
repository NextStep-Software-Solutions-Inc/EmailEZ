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

        RuleFor(x => x.SmtpHost)
            .NotEmpty().WithMessage("SMTP Host is required.")
            .MaximumLength(255).WithMessage("SMTP Host must not exceed 255 characters.");

        RuleFor(x => x.SmtpPort)
            .InclusiveBetween(1, 65535).WithMessage("SMTP Port must be a valid port number (1-65535).");

        RuleFor(x => x.SmtpUsername)
            .NotEmpty().WithMessage("SMTP Username is required.")
            .MaximumLength(255).WithMessage("SMTP Username must not exceed 255 characters.");

        RuleFor(x => x.SmtpPassword)
            .NotEmpty().WithMessage("SMTP Password is required.");
        // We don't typically set a max length here as it will be encrypted and stored as TEXT
        // You might add complexity rules if needed, but for an SMTP password, it's provided by the client.

        RuleFor(x => x.SmtpEnableSsl)
            .NotNull().WithMessage("SMTP Enable SSL must be specified.");
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