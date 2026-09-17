using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DMTacvu")]
public class TaskType
{
    [Key]
    [Column("Matacvu")]
    public int Id { get; set; }

    [Column("Tentacvu")]
    public string? Name { get; set; }
}
