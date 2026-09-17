namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceCodeDto
{
    public int Id { get; set; }
    public string? QrCode { get; set; }
    public string? Nfc { get; set; }
}
