using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DanhmucDV")]
public class OrganizationType
{
    [Key]
    [Column("MaloaiDV")]
    public int Id { get; set; }

    [Column("TenloaiDV")]
    public string? Name { get; set; }
}
