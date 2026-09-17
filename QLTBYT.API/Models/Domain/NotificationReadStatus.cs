using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Thongbaostatus")]
public class NotificationReadStatus
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDthongbao")]
    public int? NotificationId { get; set; }

    [Column("IDuser")]
    public int? UserId { get; set; }

    [Column("read_status")]
    public int? ReadStatus { get; set; }

    public virtual Notification? Notification { get; set; }
}
