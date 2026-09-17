using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("QuanlyNhansu")]
public class Personnel
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Hoten")]
    public string? FullName { get; set; }

    [Column("SoCMND")]
    public string? IdentityNumber { get; set; }

    [Column("Dienthoai")]
    public string? Phone { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("MaDV")]
    public int? OrganizationId { get; set; }

    [Column("LoaiNhansu")]
    public int? PersonnelTypeId { get; set; }

    [Column("IDTaikhoan")]
    public int? UserAccountId { get; set; }
}
