using FluentValidation;
using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Ten dang nhap khong duoc de trống.");
        RuleFor(x => x.ResetToken).NotEmpty().WithMessage("Ma dat lai khong hop le.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mat khau moi khong duoc de trống.")
            .MinimumLength(6).WithMessage("Mat khau moi phai co it nhat 6 ky tu.");
    }
}
