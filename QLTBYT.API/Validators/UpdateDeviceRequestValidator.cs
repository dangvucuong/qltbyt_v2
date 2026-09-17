using FluentValidation;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Validators;

public class UpdateDeviceRequestValidator : AbstractValidator<UpdateDeviceRequest>
{
    public UpdateDeviceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ten thiet bi khong duoc de trống.")
            .MaximumLength(500);

        RuleFor(x => x.TypeId).GreaterThan(0).WithMessage("Loai thiet bi khong hop le.");
        RuleFor(x => x.OrganizationId).GreaterThan(0).WithMessage("Don vi su dung khong hop le.");
        RuleFor(x => x.StatusId).GreaterThan(0).WithMessage("Trang thai khong hop le.");
    }
}
