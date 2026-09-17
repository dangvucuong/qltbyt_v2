using QLTBYT.API.Data;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Services;

public interface ISystemAuditService
{
    Task WriteAsync(int? userId, string content, int? deviceId = null);
}

public class SystemAuditService : ISystemAuditService
{
    private readonly AppDbContext _context;

    public SystemAuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task WriteAsync(int? userId, string content, int? deviceId = null)
    {
        _context.SystemAuditLogs.Add(new SystemAuditLog
        {
            UserId = userId,
            Content = content,
            Timestamp = DateTime.Now,
            DeviceId = deviceId
        });
        await _context.SaveChangesAsync();
    }
}
