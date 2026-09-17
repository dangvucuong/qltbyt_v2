using FluentValidation;
using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Ten dang nhap khong duoc de trống.")
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mat khau khong duoc de trống.")
            .MinimumLength(6).WithMessage("Mat khau phai co it nhat 6 ky tu.");
    }
}
