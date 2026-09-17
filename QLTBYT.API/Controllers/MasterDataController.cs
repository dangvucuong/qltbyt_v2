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
    public async Task<IActionResult> GetDeviceTypes()
    {
        var data = await _masterDataService.GetDeviceTypesAsync();
        return Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(data));
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatuses()
    {
        var data = await _masterDataService.GetDeviceStatusesAsync();
        return Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(data));
    }

    [HttpGet("org-types")]
    public async Task<IActionResult> GetOrgTypes()
    {
        var data = await _masterDataService.GetOrganizationTypesAsync();
        return Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(data));
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries()
    {
        var data = await _masterDataService.GetCountriesAsync();
        return Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(data));
    }

    [HttpGet("task-types")]
    public async Task<IActionResult> GetTaskTypes()
    {
        var data = await _masterDataService.GetTaskTypesAsync();
        return Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(data));
    }

    [HttpGet("event-keywords")]
    public async Task<IActionResult> GetEventKeywords()
    {
        var data = await _masterDataService.GetEventKeywordsAsync();
        return Ok(ApiResponse<List<MasterItemDto>>.CreateSuccess(data));
    }

    [HttpGet("bidding-packages")]
    public async Task<IActionResult> GetBiddingPackages()
    {
        var data = await _masterDataService.GetBiddingPackagesAsync();
        return Ok(ApiResponse<List<BiddingPackageLookupDto>>.CreateSuccess(data));
    }
}
