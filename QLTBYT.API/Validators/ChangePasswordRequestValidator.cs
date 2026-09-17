using FluentValidation;
using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Mat khau cu khong duoc de trống.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mat khau moi khong duoc de trống.")
            .MinimumLength(6).WithMessage("Mat khau moi phai co it nhat 6 ky tu.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Mat khau moi phai khac mat khau cu.");
    }
}
