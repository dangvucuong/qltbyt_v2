using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DanhmucLoaiNS")]
public class PersonnelType
{
    [Key]
    [Column("MaloaiNS")]
    public int Id { get; set; }

    [Column("TenloaiNS")]
    public string? Name { get; set; }
}
