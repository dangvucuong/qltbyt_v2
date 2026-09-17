using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Quanlythietbi")]
public class Device
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("MaTB")]
    public string? DeviceCode { get; set; }

    [Column("MaKT")]
    public string? TechnicalCode { get; set; }

    [Column("Madinhdanh")]
    public string? IdentityCode { get; set; }

    [Column("TenTB")]
    public string? Name { get; set; }

    [Column("Loai")]
    public int? TypeId { get; set; }

    [Column("Kyhieu")]
    public string? ModelCode { get; set; }

    [Column("Serial")]
    public string? Serial { get; set; }

    [Column("ThongsoKT")]
    public string? TechnicalSpecs { get; set; }

    [Column("Ngaytiepnhan")]
    public DateTime? ReceivedDate { get; set; }

    [Column("Donvibaotri")]
    public int? MaintenanceOrgId { get; set; }

    [Column("Donvibangiao")]
    public int? HandoverOrgId { get; set; }

    [Column("Donvisudung")]
    public int? OrganizationId { get; set; }

    [Column("Nguoiquanly")]
    public int? ManagerUserId { get; set; }

    [Column("Thoigianbh")]
    public string? WarrantyPeriod { get; set; }

    [Column("HangSX")]
    public string? Manufacturer { get; set; }

    [Column("NuocSX")]
    public string? CountryOfOrigin { get; set; }

    [Column("NamSX")]
    public string? YearOfManufacture { get; set; }

    [Column("Vattudikem")]
    public string? Accessories { get; set; }

    [Column("Trangthai")]
    public int? StatusId { get; set; }

    [Column("Tinhtrang")]
    public string? ConditionNote { get; set; }

    [Column("Ngaycapnhat")]
    public DateTime? UpdatedAt { get; set; }

    [Column("Ghichu")]
    public string? Note { get; set; }

    [Column("Thietbichinh")]
    public string? ParentDeviceCode { get; set; }

    [Column("Hankiemdinh")]
    public DateTime? InspectionExpiryDate { get; set; }

    
    [Column("Thanhtien")]
    public string? TotalAmount { get; set; }

    [Column("Dongia")]
    public string? UnitPrice { get; set; }

    [Column("MocTGBT")]
    public string? MaintenanceDueMarker { get; set; }

    [Column("Phanloai")]
    public string? Classification { get; set; }

    [Column("Nguonmuasam")]
    public string? ProcurementSource { get; set; }

    [Column("NDXuly")]
    public string? LocationCode { get; set; }

    [Column("Phannhom")]
    public string? DeviceGroup { get; set; }

    [Column("dvt")]
    public string? Unit { get; set; }

    [Column("Soluong")]
    public double? Quantity { get; set; }

    [Column("Donvicungcap")]
    public int? SupplierOrgId { get; set; }

    [Column("idGoithau")]
    public string? BiddingPackageCode { get; set; }

    [Column("IDHoso")]
    public string? LegalDocumentRef { get; set; }

    public virtual Organization? Organization { get; set; }
    public virtual DeviceType? DeviceType { get; set; }
    public virtual DeviceStatus? DeviceStatus { get; set; }
    public virtual ICollection<DeviceFile> Files { get; set; } = new List<DeviceFile>();
    public virtual ICollection<DeviceCode> Codes { get; set; } = new List<DeviceCode>();
}
