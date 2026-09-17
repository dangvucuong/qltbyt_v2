using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Country")]
public class Country
{
    [Key]
    [Column("CountryID")]
    public int Id { get; set; }

    [Column("CountryName")]
    public string? Name { get; set; }

    [Column("TwoCharCountryCode")]
    public string? TwoCharCode { get; set; }

    [Column("ThreeCharCountryCode")]
    public string? ThreeCharCode { get; set; }
}
