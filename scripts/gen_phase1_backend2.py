# -*- coding: utf-8 -*-
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def w(rel, content):
    p = ROOT / rel.replace("/", "\\")
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content.strip() + "\n", encoding="utf-8", newline="\n")
    print("ok", rel)

w("QLTBYT.API/Services/IDeviceService.cs", """
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Services;

public interface IDeviceService
{
    Task<PagedResult<DeviceListDto>> GetDevicesAsync(DeviceQueryParams query);
    Task<DeviceDetailDto?> GetDeviceByIdAsync(int id);
    Task<DeviceDetailDto> CreateDeviceAsync(CreateDeviceRequest request);
    Task<DeviceDetailDto?> UpdateDeviceAsync(int id, UpdateDeviceRequest request);
    Task<bool> DeleteDeviceAsync(int id);
    Task<List<DeviceListDto>> GetChildrenAsync(int parentId);
    Task<DeviceFileDto> AddFileAsync(int deviceId, Stream content, string fileName, string? description);
}
""")

w("QLTBYT.API/Controllers/MasterDataController.cs", """
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Master;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/master")]
[Authorize(Policy = "AllUsers")]
public class MasterDataController : ControllerBase
{
    private readonly IMasterDataService _masterDataService;

    public MasterDataController(IMasterDataService masterDataService)
    {
        _masterDataService = masterDataService;
    }

    [HttpGet("device-types")]
    public async Task<ActionResult<ApiResponse<List<MasterItemDto>>> GetDeviceTypes()
        => Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(await _masterDataService.GetDeviceTypesAsync()));

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<List<MasterItemDto>>> GetStatuses()
        => Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(await _masterDataService.GetDeviceStatusesAsync()));

    [HttpGet("org-types")]
    public async Task<ActionResult<ApiResponse<List<MasterItemDto>>> GetOrgTypes()
        => Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(await _masterDataService.GetOrganizationTypesAsync()));

    [HttpGet("countries")]
    public async Task<ActionResult<ApiResponse<List<MasterItemDto>>> GetCountries()
        => Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(await _masterDataService.GetCountriesAsync()));

    [HttpGet("task-types")]
    public async Task<ActionResult<ApiResponse<List<MasterItemDto>>> GetTaskTypes()
        => Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(await _masterDataService.GetTaskTypesAsync()));

    [HttpGet("event-keywords")]
    public async Task<ActionResult<ApiResponse<List<MasterItemDto>>> GetEventKeywords()
        => Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(await _masterDataService.GetEventKeywordsAsync()));
}
""")

w("QLTBYT.API/Controllers/OrganizationsController.cs", """
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Organization;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize(Policy = "AllUsers")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrganizationListDto>>> GetOrganizations()
    {
        var list = await _organizationService.GetOrganizationsAsync();
        return Ok(ApiResponse<List<OrganizationListDto>>.CreateSuccess(list));
    }
}
""")

w("QLTBYT.API/Controllers/DashboardController.cs", """
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Dashboard;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "AllUsers")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary([FromQuery] int expiringWithinDays = 30)
    {
        var data = await _dashboardService.GetSummaryAsync(expiringWithinDays);
        return Ok(ApiResponse<DashboardSummaryDto>.CreateSuccess(data));
    }
}
""")

print("batch2 done")
