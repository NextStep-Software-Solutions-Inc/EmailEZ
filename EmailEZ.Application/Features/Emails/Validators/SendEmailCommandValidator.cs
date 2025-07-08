using EmailEZ.Application.Features.Emails.Commands.SendEmail;
using EmailEZ.Application.Features.Workspaces.Commands.CreateWorkspace;
using FluentValidation;

namespace EmailEZ.Application.Features.Workspaces.Valicators;

/// <summary>
/// Validator for the CreateWorkspaceCommand.
/// </summary>
public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator()
    {
        RuleFor(x => x.ToEmail)
            .NotEmpty().WithMessage("At least one recipient email is required.")
            .Must(emails => emails.Count > 0).WithMessage("At least one recipient email is required.")
            .ForEach(email => email
                .NotEmpty().WithMessage("Recipient email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format."));

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(255).WithMessage("Subject must not exceed 255 characters.");
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Email body is required.")
            .MaximumLength(5000).WithMessage("Email body must not exceed 5000 characters.");
        RuleFor(x => x.FromDisplayName)
            .MaximumLength(255).WithMessage("From display name must not exceed 255 characters.");
        RuleFor(x => x.EmailConfigurationId)
            .NotEmpty().WithMessage("Email configuration ID is required.")
            .Must(ecid => ecid != Guid.Empty).WithMessage("Email configuration ID cannot be an empty GUID.");
        RuleFor(x => x.CcEmail)
            .Must(cc => cc == null || cc.Count <= 50).WithMessage("CC list must not exceed 50 emails if provided.")
            .ForEach(ccEmail => ccEmail
                .NotEmpty().WithMessage("CC email cannot be empty.")
                .EmailAddress().WithMessage("Invalid CC email format."));
        RuleFor(x => x.BccEmail)
            .Must(bcc => bcc == null || bcc.Count <= 50).WithMessage("BCC list must not exceed 50 emails if provided.")
            .ForEach(bccEmail => bccEmail
                .NotEmpty().WithMessage("BCC email cannot be empty.")
                .EmailAddress().WithMessage("Invalid BCC email format."));

    }
}