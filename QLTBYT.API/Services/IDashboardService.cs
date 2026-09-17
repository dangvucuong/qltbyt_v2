using QLTBYT.API.Models.DTOs.Dashboard;

namespace QLTBYT.API.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int expiringWithinDays = 30);
}
