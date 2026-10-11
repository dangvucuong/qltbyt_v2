# -*- coding: utf-8 -*-
from pathlib import Path

API = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API")

# --- Device.cs: add missing columns before Organization nav ---
device = (API / "Models/Domain/Device.cs").read_text(encoding="utf-8")
if "WarrantyDueMarker" not in device:
    insert = '''
    [Column("MocTGBH")]
    public string? WarrantyDueMarker { get; set; }

    [Column("MocTGHieuchuan")]
    public string? CalibrationDueMarker { get; set; }

    [Column("MocTGKiemdinh")]
    public string? InspectionDueMarker { get; set; }

    [Column("Thoigianduavaosd")]
    public string? PutIntoUseMarker { get; set; }

    [Column("Tenviettat")]
    public string? SpecializedName { get; set; }

'''
    device = device.replace(
        "    [Column(\"IDHoso\")]\n    public string? LegalDocumentRef { get; set; }\n\n    public virtual Organization?",
        "    [Column(\"IDHoso\")]\n    public string? LegalDocumentRef { get; set; }\n" + insert + "    public virtual Organization?",
    )
    (API / "Models/Domain/Device.cs").write_text(device, encoding="utf-8", newline="\n")
    print("Device.cs updated")
else:
    print("Device.cs already has markers")

fields_detail = '''
    public string? TechnicalSpecs { get; set; }
    public string? YearOfManufacture { get; set; }
    public string? ConditionNote { get; set; }
    public string? DeviceGroup { get; set; }
    public string? Classification { get; set; }
    public int? HandoverOrgId { get; set; }
    public int? MaintenanceOrgId { get; set; }
    public string? BiddingPackageCode { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? UnitPrice { get; set; }
    public string? TotalAmount { get; set; }
    public string? LocationCode { get; set; }
    public string? ProcurementSource { get; set; }
    public string? WarrantyDueMarker { get; set; }
    public string? MaintenanceDueMarker { get; set; }
    public string? CalibrationDueMarker { get; set; }
    public string? InspectionDueMarker { get; set; }
    public string? PutIntoUseMarker { get; set; }
    public string? SpecializedName { get; set; }
'''

detail = '''namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceDetailDto
{
    public int Id { get; set; }
    public string? DeviceCode { get; set; }
    public string? TechnicalCode { get; set; }
    public string? IdentityCode { get; set; }
    public string? Name { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public string? Manufacturer { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? ParentDeviceCode { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public string? Note { get; set; }
    public DateTime? UpdatedAt { get; set; }
''' + fields_detail + '''
    public List<DeviceFileDto> Files { get; set; } = new List<DeviceFileDto>();
    public List<DeviceCodeDto> Codes { get; set; } = new List<DeviceCodeDto>();
}
'''
(API / "Models/DTOs/Device/DeviceDetailDto.cs").write_text(detail, encoding="utf-8", newline="\n")

create = '''namespace QLTBYT.API.Models.DTOs.Device;

public class CreateDeviceRequest
{
    public string DeviceCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public int OrganizationId { get; set; }
    public int StatusId { get; set; }
    public string? IdentityCode { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public string? Manufacturer { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? YearOfManufacture { get; set; }
    public string? TechnicalSpecs { get; set; }
    public string? ConditionNote { get; set; }
    public string? DeviceGroup { get; set; }
    public string? Classification { get; set; }
    public int? HandoverOrgId { get; set; }
    public int? MaintenanceOrgId { get; set; }
    public string? BiddingPackageCode { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? UnitPrice { get; set; }
    public string? TotalAmount { get; set; }
    public string? LocationCode { get; set; }
    public string? ProcurementSource { get; set; }
    public string? WarrantyDueMarker { get; set; }
    public string? MaintenanceDueMarker { get; set; }
    public string? CalibrationDueMarker { get; set; }
    public string? InspectionDueMarker { get; set; }
    public string? PutIntoUseMarker { get; set; }
    public string? SpecializedName { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public string? Note { get; set; }
    public string? ParentDeviceCode { get; set; }
}
'''
(API / "Models/DTOs/Device/CreateDeviceRequest.cs").write_text(create, encoding="utf-8", newline="\n")

update = '''namespace QLTBYT.API.Models.DTOs.Device;

public class UpdateDeviceRequest
{
    public string Name { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public int OrganizationId { get; set; }
    public int StatusId { get; set; }
    public string? IdentityCode { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public string? Manufacturer { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? YearOfManufacture { get; set; }
    public string? TechnicalSpecs { get; set; }
    public string? ConditionNote { get; set; }
    public string? DeviceGroup { get; set; }
    public string? Classification { get; set; }
    public int? HandoverOrgId { get; set; }
    public int? MaintenanceOrgId { get; set; }
    public string? BiddingPackageCode { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? UnitPrice { get; set; }
    public string? TotalAmount { get; set; }
    public string? LocationCode { get; set; }
    public string? ProcurementSource { get; set; }
    public string? WarrantyDueMarker { get; set; }
    public string? MaintenanceDueMarker { get; set; }
    public string? CalibrationDueMarker { get; set; }
    public string? InspectionDueMarker { get; set; }
    public string? PutIntoUseMarker { get; set; }
    public string? SpecializedName { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public string? Note { get; set; }
    public string? ParentDeviceCode { get; set; }
}
'''
(API / "Models/DTOs/Device/UpdateDeviceRequest.cs").write_text(update, encoding="utf-8", newline="\n")

create_val = '''using FluentValidation;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Validators;

public class CreateDeviceRequestValidator : AbstractValidator<CreateDeviceRequest>
{
    public CreateDeviceRequestValidator()
    {
        RuleFor(x => x.DeviceCode)
            .NotEmpty().WithMessage("Mã thiết bị không được để trống.")
            .MaximumLength(50).WithMessage("Mã thiết bị tối đa 50 ký tự.");

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
'''
(API / "Validators/CreateDeviceRequestValidator.cs").write_text(create_val, encoding="utf-8", newline="\n")

update_val = '''using FluentValidation;
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
'''
(API / "Validators/UpdateDeviceRequestValidator.cs").write_text(update_val, encoding="utf-8", newline="\n")

# DeviceService VN messages
svc = (API / "Services/DeviceService.cs").read_text(encoding="utf-8")
svc = svc.replace('throw new InvalidOperationException("Ma thiet bi da ton tai.");',
                  'throw new InvalidOperationException("Mã thiết bị đã tồn tại.");')
svc = svc.replace('throw new InvalidOperationException("Khong the doc thiet bi sau khi tao.");',
                  'throw new InvalidOperationException("Không đọc được thiết bị sau khi tạo.");')
svc = svc.replace('throw new InvalidOperationException("Khong tim thay thiet bi.");',
                  'throw new InvalidOperationException("Không tìm thấy thiết bị.");')
(API / "Services/DeviceService.cs").write_text(svc, encoding="utf-8", newline="\n")
print("backend DTOs/validators ok")
