namespace QLTBYT.API.Models.DTOs.Health;

public class DatabaseHealthDto
{
    public string DatabaseName { get; set; } = string.Empty;
    public bool CanConnect { get; set; }
    public int TableCount { get; set; }
    public bool CoreTablesPresent { get; set; }
    public IReadOnlyList<string> MissingCoreTables { get; set; } = Array.Empty<string>();
}
