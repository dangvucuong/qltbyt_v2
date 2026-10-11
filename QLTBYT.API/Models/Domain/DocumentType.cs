using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DanhmucHoso")]
public class DocumentType
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Tenhienthi")]
    public string? Name { get; set; }

    [Column("LinkHS")]
    public string? Link { get; set; }
}
