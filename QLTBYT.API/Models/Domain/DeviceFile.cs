using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Quanlyfile")]
public class DeviceFile
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Path")]
    public string? Path { get; set; }

    [Column("Mota")]
    public string? Description { get; set; }

    [Column("idTB")]
    public int? DeviceId { get; set; }

    [Column("Ngaythang")]
    public DateTime? FileDate { get; set; }

    public virtual Device? Device { get; set; }
}
