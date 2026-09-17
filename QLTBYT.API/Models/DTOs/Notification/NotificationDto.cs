namespace QLTBYT.API.Models.DTOs.Notification;

public class NotificationDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
