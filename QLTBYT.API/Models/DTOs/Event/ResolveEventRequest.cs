namespace QLTBYT.API.Models.DTOs.Event;

public class ResolveEventRequest
{
    public string ResolutionContent { get; set; } = string.Empty;
    public double? Cost { get; set; }
}
