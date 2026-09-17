using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Notification;

namespace QLTBYT.API.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetForCurrentUserAsync();
    Task MarkReadAsync(int notificationId);
    Task MarkAllReadAsync();
    Task NotifyUsersAsync(string title, string content, int createdByUserId, IEnumerable<int> targetUserIds);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public NotificationService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<NotificationDto>> GetForCurrentUserAsync()
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return new List<NotificationDto>();
        }

        var rows = await _context.NotificationReadStatuses
            .AsNoTracking()
            .Include(r => r.Notification)
            .Where(r => r.UserId == userId.Value)
            .OrderByDescending(r => r.Notification!.CreatedAt)
            .Take(50)
            .ToListAsync();

        return rows.Select(r => new NotificationDto
        {
            Id = r.NotificationId ?? 0,
            Title = r.Notification?.Title?.Trim(),
            Content = r.Notification?.Content?.Trim(),
            CreatedAt = r.Notification?.CreatedAt,
            IsRead = r.ReadStatus == 1
        }).ToList();
    }

    public async Task MarkReadAsync(int notificationId)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return;
        }

        var row = await _context.NotificationReadStatuses
            .FirstOrDefaultAsync(r => r.NotificationId == notificationId && r.UserId == userId.Value);
        if (row is null)
        {
            return;
        }

        row.ReadStatus = 1;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync()
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return;
        }

        var rows = await _context.NotificationReadStatuses
            .Where(r => r.UserId == userId.Value && r.ReadStatus != 1)
            .ToListAsync();
        if (rows.Count == 0)
        {
            return;
        }

        foreach (var row in rows)
        {
            row.ReadStatus = 1;
        }

        await _context.SaveChangesAsync();
    }

    public async Task NotifyUsersAsync(string title, string content, int createdByUserId, IEnumerable<int> targetUserIds)
    {
        var nextNotificationId = (await _context.Notifications.MaxAsync(n => (int?)n.Id) ?? 0) + 1;
        var nextStatusId = (await _context.NotificationReadStatuses.MaxAsync(s => (int?)s.Id) ?? 0);

        _context.Notifications.Add(new Notification
        {
            Id = nextNotificationId,
            Title = title,
            Content = content,
            CreatedAt = DateTime.Now,
            CreatedByUserId = createdByUserId.ToString(),
            Status = 1
        });

        foreach (var uid in targetUserIds.Distinct())
        {
            nextStatusId++;
            _context.NotificationReadStatuses.Add(new NotificationReadStatus
            {
                Id = nextStatusId,
                NotificationId = nextNotificationId,
                UserId = uid,
                ReadStatus = 0
            });
        }

        await _context.SaveChangesAsync();
    }
}
