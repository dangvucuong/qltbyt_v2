using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DMLoaiTB")]
public class DeviceType
{
    [Key]
    [Column("MaloaiTB")]
    public int Id { get; set; }

    [Column("TenloaiTB")]
    public string? Name { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
