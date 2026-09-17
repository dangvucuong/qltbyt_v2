using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("QuanlyMa")]
public class DeviceCode
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDTB")]
    public int? DeviceId { get; set; }

    [Column("QRCODE")]
    public string? QrCode { get; set; }

    [Column("NFC")]
    public string? Nfc { get; set; }

    [Column("IDNguoiSD")]
    public int? AssignedUserId { get; set; }

    public virtual Device? Device { get; set; }
}
