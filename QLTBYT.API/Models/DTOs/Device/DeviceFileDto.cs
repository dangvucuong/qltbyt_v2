namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceFileDto
{
    public int Id { get; set; }
    public string? Path { get; set; }
    public string? Description { get; set; }
    public DateTime? FileDate { get; set; }
}
