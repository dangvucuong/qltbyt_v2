using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("QuanlyDV")]
public class Organization
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Tendonvi")]
    public string? Name { get; set; }

    [Column("Masothue")]
    public string? TaxCode { get; set; }

    [Column("Diachi")]
    public string? Address { get; set; }

    [Column("Dienthoai")]
    public string? Phone { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("MaLoai")]
    public int? TypeId { get; set; }

    public virtual OrganizationType? OrganizationType { get; set; }
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
