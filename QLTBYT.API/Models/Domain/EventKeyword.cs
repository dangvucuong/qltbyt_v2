using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("EventKeyword")]
public class EventKeyword
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Keyword")]
    public string Keyword { get; set; } = string.Empty;
}
