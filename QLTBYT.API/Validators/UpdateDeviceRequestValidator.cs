using FluentValidation;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Validators;

public class UpdateDeviceRequestValidator : AbstractValidator<UpdateDeviceRequest>
{
    public UpdateDeviceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên thiết bị không được để trống.")
            .MaximumLength(500).WithMessage("Tên thiết bị tối đa 500 ký tự.");

        RuleFor(x => x.TypeId).GreaterThan(0).WithMessage("Loại thiết bị không hợp lệ.");
        RuleFor(x => x.OrganizationId).GreaterThan(0).WithMessage("Đơn vị sử dụng không hợp lệ.");
        RuleFor(x => x.StatusId).GreaterThan(0).WithMessage("Trạng thái không hợp lệ.");

        RuleFor(x => x.IdentityCode).MaximumLength(100).When(x => x.IdentityCode != null);
        RuleFor(x => x.Classification).MaximumLength(20).When(x => x.Classification != null);
        RuleFor(x => x.LocationCode).MaximumLength(50).When(x => x.LocationCode != null);
        RuleFor(x => x.ProcurementSource).MaximumLength(50).When(x => x.ProcurementSource != null);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).When(x => x.Quantity.HasValue)
            .WithMessage("Số lượng không được âm.");
        RuleFor(x => x.MaintenanceOrgId).InclusiveBetween(1, 4).When(x => x.MaintenanceOrgId.HasValue)
            .WithMessage("Hình thức bảo trì không hợp lệ.");
    }
}
