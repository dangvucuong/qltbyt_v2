using QLTBYT.API.Models.DTOs.Health;

namespace QLTBYT.API.Services;

public interface IDatabaseHealthService
{
    Task<DatabaseHealthDto> CheckAsync();
}