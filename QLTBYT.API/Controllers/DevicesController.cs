using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Event;
using QLTBYT.API.Models.DTOs.Legal;
using QLTBYT.API.Models.DTOs.UsageLog;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize(Policy = "AllUsers")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceEventService _deviceEventService;
    private readonly ILegalDocumentService _legalDocumentService;
    private readonly IFileStorageService _fileStorage;
    private readonly IPdfSignatureService _pdfSignature;
    private readonly IUsageLogService _usageLogService;

    public DevicesController(
        IDeviceService deviceService,
        IDeviceEventService deviceEventService,
        ILegalDocumentService legalDocumentService,
        IFileStorageService fileStorage,
        IPdfSignatureService pdfSignature,
        IUsageLogService usageLogService)
    {
        _deviceService = deviceService;
        _deviceEventService = deviceEventService;
        _legalDocumentService = legalDocumentService;
        _fileStorage = fileStorage;
        _pdfSignature = pdfSignature;
        _usageLogService = usageLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DeviceListDto>>>> GetDevices(
        [FromQuery] DeviceQueryParams queryParams)
    {
        var result = await _deviceService.GetDevicesAsync(queryParams);
        return Ok(ApiResponse<PagedResult<DeviceListDto>>.CreateSuccess(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> GetDevice(int id)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device is null)
        {
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Không tìm thấy thiết bị."));
        }

        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(device));
    }

    [HttpGet("{id:int}/children")]
    public async Task<IActionResult> GetChildren(int id)
    {
        var children = await _deviceService.GetChildrenAsync(id);
        return Ok(ApiResponse<List<DeviceListDto>>.CreateSuccess(children));
    }

    [HttpPost]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> CreateDevice([FromBody] CreateDeviceRequest request)
    {
        try
        {
            var device = await _deviceService.CreateDeviceAsync(request);
            return CreatedAtAction(nameof(GetDevice), new { id = device.Id },
                ApiResponse<DeviceDetailDto>.CreateSuccess(device, "Thêm thiết bị thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DeviceDetailDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AllUsers")]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> UpdateDevice(int id, [FromBody] UpdateDeviceRequest request)
    {
        var device = await _deviceService.UpdateDeviceAsync(id, request);
        if (device is null)
        {
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Không tìm thấy thiết bị."));
        }

        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(device, "Cập nhật thiết bị thành công."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDevice(int id)
    {
        var ok = await _deviceService.DeleteDeviceAsync(id);
        if (!ok)
        {
            return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy thiết bị."));
        }

        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa thiết bị thành công."));
    }

    [HttpPost("{id:int}/files")]
    [Authorize(Policy = "TechnicianAndAbove")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<DeviceFileDto>>> UploadFile(
        int id,
        IFormFile file,
        [FromForm] string? description)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail("File không hợp lệ."));
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var dto = await _deviceService.AddFileAsync(id, stream, file.FileName, description);
            return Ok(ApiResponse<DeviceFileDto>.CreateSuccess(dto, "Upload file thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<DeviceFileDto>.CreateFail(ex.Message));
        }
    }

    /// <summary>
    /// Upload hợp đồng bảo trì (legacy: HDBaotri/{id}/ + lưu tên file vào IDHoso).
    /// </summary>
    [HttpPost("{id:int}/maintenance-contract")]
    [Authorize(Policy = "TechnicianAndAbove")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<object>>> UploadMaintenanceContract(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.CreateFail("File không hợp lệ."));
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var relativePath = await _deviceService.UploadMaintenanceContractAsync(id, stream, file.FileName);
            var fileName = Path.GetFileName(relativePath);
            return Ok(ApiResponse<object>.CreateSuccess(
                new { path = relativePath, fileName },
                "Upload hợp đồng bảo trì thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.CreateFail(ex.Message));
        }
    }

    [HttpPost("{id:int}/events")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> CreateDeviceEvent(
        int id,
        [FromBody] CreateDeviceEventRequest request)
    {
        try
        {
            var dto = await _deviceEventService.CreateForDeviceAsync(id, request);
            return Ok(ApiResponse<EventDetailDto>.CreateSuccess(dto, "Gửi yêu cầu thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EventDetailDto>.CreateFail(ex.Message));
        }
    }

    /// <summary>
    /// Upload hình ảnh sự cố / báo cáo sự cố (legacy: TAILIEU/{id}/Hinh_anh_su_co|BAO_CAO_SU_CO).
    /// kind: image | report
    /// </summary>
    [HttpGet("{id:int}/incident-files")]
    public async Task<ActionResult<ApiResponse<List<DeviceFileDto>>>> GetIncidentFiles(
        int id,
        [FromQuery] string? kind = null)
    {
        var list = await _deviceEventService.GetIncidentFilesForDeviceAsync(id, kind);
        return Ok(ApiResponse<List<DeviceFileDto>>.CreateSuccess(list));
    }

    [HttpPost("{id:int}/incident-files")]
    [Authorize(Policy = "AllUsers")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<DeviceFileDto>>> UploadIncidentFile(
        int id,
        IFormFile file,
        [FromForm] string kind = "image")
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail("File không hợp lệ."));

        try
        {
            await using var stream = file.OpenReadStream();
            var dto = await _deviceEventService.AddIncidentFileForDeviceAsync(id, stream, file.FileName, kind);
            return Ok(ApiResponse<DeviceFileDto>.CreateSuccess(dto, "Upload file sự cố thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail(ex.Message));
        }
    }

    [HttpGet("{id:int}/legal-document")]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> GetLegalDocument(int id)
    {
        var doc = await _legalDocumentService.GetForDeviceAsync(id);
        if (doc is null)
            return NotFound(ApiResponse<LegalDocumentDto>.CreateFail("Chưa có hồ sơ pháp lý cho thiết bị này."));
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(doc));
    }

    [HttpPut("{id:int}/legal-document")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<object>>> LinkLegalDocument(int id, [FromBody] LinkDeviceLegalRequest request)
    {
        var ok = await _legalDocumentService.LinkDeviceAsync(id, request.LegalDocumentId);
        if (!ok)
            return NotFound(ApiResponse<object>.CreateFail("Không liên kết được hồ sơ / thiết bị."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Liên kết hồ sơ pháp lý thành công."));
    }
    
    [HttpPost("{id:int}/captures")]
    [Authorize(Policy = "TechnicianAndAbove")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<DeviceFileDto>>> CapturePhoto(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail("File không hợp lệ."));
        await using var stream = file.OpenReadStream();
        var path = await _fileStorage.SaveCaptureAsync(id, stream, file.FileName);
        var dto = await _deviceService.AddFileWithPathAsync(id, path, "Ảnh chụp từ camera");
        return Ok(ApiResponse<DeviceFileDto>.CreateSuccess(dto, "Lưu ảnh chụp thành công."));
    }

    [HttpPost("files/{fileId:int}/sign-pdf")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<DeviceFileDto>>> SignPdf(int fileId)
    {
        var signer = User.Identity?.Name ?? "Kỹ thuật";
        try
        {
            var dto = await _pdfSignature.SignDeviceFileAsync(fileId, signer);
            if (dto is null) return NotFound(ApiResponse<DeviceFileDto>.CreateFail("Không tìm thấy file."));
            return Ok(ApiResponse<DeviceFileDto>.CreateSuccess(dto, "Ký số PDF thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail(ex.Message));
        }
    }
    [HttpPost("{id:int}/transfer")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> Transfer(int id, [FromBody] TransferDeviceRequest request)
    {
        var dto = await _deviceService.TransferDeviceAsync(id, request);
        if (dto is null)
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Không tìm thấy thiết bị."));
        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(dto, "Chuyển giao thành công."));
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DeviceHistoryItemDto>>>> GetHistory(int id)
    {
        var list = await _deviceService.GetDeviceHistoryAsync(id);
        return Ok(ApiResponse<IReadOnlyList<DeviceHistoryItemDto>>.CreateSuccess(list));
    }

    [HttpPut("{id:int}/qr")]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> UpdateQr(int id, [FromBody] UpdateDeviceQrRequest request)
    {
        var dto = await _deviceService.UpdateQrAsync(id, request);
        if (dto is null)
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Không tìm thấy thiết bị."));
        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(dto, "Cập nhật mã QR/NFC thành công."));
    }

    [HttpGet("{id:int}/supplies")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DeviceSupplyDto>>>> GetSupplies(int id)
    {
        var list = await _deviceService.GetSuppliesAsync(id);
        return Ok(ApiResponse<IReadOnlyList<DeviceSupplyDto>>.CreateSuccess(list));
    }

    [HttpPost("{id:int}/supplies")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<DeviceSupplyDto>>> AddSupply(int id, [FromBody] CreateDeviceSupplyRequest request)
    {
        try
        {
            var item = await _deviceService.AddSupplyAsync(id, request);
            return Ok(ApiResponse<DeviceSupplyDto>.CreateSuccess(item));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DeviceSupplyDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("{id:int}/supplies/{supplyId:int}")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSupply(int id, int supplyId)
    {
        var ok = await _deviceService.DeleteSupplyAsync(id, supplyId);
        if (!ok) return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy vật tư."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }));
    }

    /// <summary>Nhật ký sử dụng theo thiết bị (legacy SudungTB.aspx).</summary>
    [HttpGet("{id:int}/usage-logs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UsageLogDto>>>> GetUsageLogs(
        int id,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device is null)
            return NotFound(ApiResponse<IReadOnlyList<UsageLogDto>>.CreateFail("Không tìm thấy thiết bị."));

        var deviceKey = !string.IsNullOrWhiteSpace(device.DeviceCode)
            ? device.DeviceCode!.Trim()
            : id.ToString();

        var list = await _usageLogService.GetLogsAsync(new UsageLogQueryParams
        {
            DeviceCode = deviceKey,
            FromDate = fromDate,
            ToDate = toDate,
        });
        return Ok(ApiResponse<IReadOnlyList<UsageLogDto>>.CreateSuccess(list));
    }

    [HttpPost("{id:int}/usage-logs")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<UsageLogDto>>> UpsertUsageLog(
        int id,
        [FromBody] CreateUsageLogRequest request)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device is null)
            return NotFound(ApiResponse<UsageLogDto>.CreateFail("Không tìm thấy thiết bị."));

        request.DeviceCode = !string.IsNullOrWhiteSpace(device.DeviceCode)
            ? device.DeviceCode!.Trim()
            : id.ToString();

        try
        {
            var item = await _usageLogService.UpsertAsync(request);
            return Ok(ApiResponse<UsageLogDto>.CreateSuccess(item, "Ghi nhật ký thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UsageLogDto>.CreateFail(ex.Message));
        }
    }

    [HttpPost("import")]
    [Authorize(Policy = "TechnicianAndAbove")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<object>>> ImportExcel(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.CreateFail("File không hợp lệ."));
        await using var stream = file.OpenReadStream();
        var count = await _deviceService.ImportDevicesFromExcelAsync(stream);
        return Ok(ApiResponse<object>.CreateSuccess(new { imported = count }, $"Import {count} thiết bị thành công."));
    }
}
