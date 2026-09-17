using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Taikhoan")]
public class UserAccount
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Tendangnhap")]
    public string? Username { get; set; }

    [Column("Matkhau")]
    public string? Password { get; set; }

    [Column("Admin")]
    public int? Admin { get; set; }

    [Column("Hoten")]
    public string? FullName { get; set; }

    [Column("Dienthoai")]
    public string? Phone { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("RegID")]
    public string? RegId { get; set; }

    [Column("LoaiTK")]
    public int? AccountType { get; set; }

    [Column("MaDV")]
    public string? OrganizationIds { get; set; }
}
