using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DMTrangthai")]
public class DeviceStatus
{
    [Key]
    [Column("MaTrangthai")]
    public int Id { get; set; }

    [Column("TenTrangthai")]
    public string? Name { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
