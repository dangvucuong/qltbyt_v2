using FluentValidation;
using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.Username))
            .WithMessage("Can nhap email hoac ten dang nhap.");
    }
}
