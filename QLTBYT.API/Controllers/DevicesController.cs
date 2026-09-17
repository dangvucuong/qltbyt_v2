using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Event;
using QLTBYT.API.Models.DTOs.Legal;
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

    public DevicesController(
        IDeviceService deviceService,
        IDeviceEventService deviceEventService,
        ILegalDocumentService legalDocumentService,
        IFileStorageService fileStorage,
        IPdfSignatureService pdfSignature)
    {
        _deviceService = deviceService;
        _deviceEventService = deviceEventService;
        _legalDocumentService = legalDocumentService;
        _fileStorage = fileStorage;
        _pdfSignature = pdfSignature;
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
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Khong tim thay thiet bi."));
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
                ApiResponse<DeviceDetailDto>.CreateSuccess(device, "Them thiet bi thanh cong."));
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
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Khong tim thay thiet bi."));
        }

        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(device, "Cap nhat thiet bi thanh cong."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDevice(int id)
    {
        var ok = await _deviceService.DeleteDeviceAsync(id);
        if (!ok)
        {
            return NotFound(ApiResponse<object>.CreateFail("Khong tim thay thiet bi."));
        }

        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xoa thiet bi thanh cong."));
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
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail("File khong hop le."));
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var dto = await _deviceService.AddFileAsync(id, stream, file.FileName, description);
            return Ok(ApiResponse<DeviceFileDto>.CreateSuccess(dto, "Upload file thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<DeviceFileDto>.CreateFail(ex.Message));
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
            return Ok(ApiResponse<EventDetailDto>.CreateSuccess(dto, "G\u1eedi y\u00eau c\u1ea7u th\u00e0nh c\u00f4ng."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EventDetailDto>.CreateFail(ex.Message));
        }
    }


    [HttpGet("{id:int}/legal-document")]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> GetLegalDocument(int id)
    {
        var doc = await _legalDocumentService.GetForDeviceAsync(id);
        if (doc is null)
            return NotFound(ApiResponse<LegalDocumentDto>.CreateFail("ChÆ°a cÃ³ há»“ sÆ¡ phÃ¡p lÃ½ cho thiáº¿t bá»‹ nÃ y."));
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(doc));
    }

    [HttpPut("{id:int}/legal-document")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<object>>> LinkLegalDocument(int id, [FromBody] LinkDeviceLegalRequest request)
    {
        var ok = await _legalDocumentService.LinkDeviceAsync(id, request.LegalDocumentId);
        if (!ok)
            return NotFound(ApiResponse<object>.CreateFail("KhÃ´ng liÃªn káº¿t Ä‘Æ°á»£c há»“ sÆ¡ / thiáº¿t bá»‹."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "LiÃªn káº¿t há»“ sÆ¡ phÃ¡p lÃ½ thÃ nh cÃ´ng."));
    }
    
    [HttpPost("{id:int}/captures")]
    [Authorize(Policy = "TechnicianAndAbove")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<DeviceFileDto>>> CapturePhoto(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail("File khong hop le."));
        await using var stream = file.OpenReadStream();
        var path = await _fileStorage.SaveCaptureAsync(id, stream, file.FileName);
        var dto = await _deviceService.AddFileWithPathAsync(id, path, "Anh chup tu camera");
        return Ok(ApiResponse<DeviceFileDto>.CreateSuccess(dto, "Luu anh chup thanh cong."));
    }

    [HttpPost("files/{fileId:int}/sign-pdf")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<DeviceFileDto>>> SignPdf(int fileId)
    {
        var signer = User.Identity?.Name ?? "Ky thuat";
        try
        {
            var dto = await _pdfSignature.SignDeviceFileAsync(fileId, signer);
            if (dto is null) return NotFound(ApiResponse<DeviceFileDto>.CreateFail("Khong tim thay file."));
            return Ok(ApiResponse<DeviceFileDto>.CreateSuccess(dto, "Ky so PDF thanh cong."));
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
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Khong tim thay thiet bi."));
        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(dto, "Chuyen giao thanh cong."));
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
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Khong tim thay thiet bi."));
        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(dto, "Cap nhat ma QR/NFC thanh cong."));
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
        if (!ok) return NotFound(ApiResponse<object>.CreateFail("Khong tim thay vat tu."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }));
    }

    [HttpPost("import")]
    [Authorize(Policy = "TechnicianAndAbove")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<object>>> ImportExcel(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.CreateFail("File khong hop le."));
        await using var stream = file.OpenReadStream();
        var count = await _deviceService.ImportDevicesFromExcelAsync(stream);
        return Ok(ApiResponse<object>.CreateSuccess(new { imported = count }, $"Import {count} thiet bi thanh cong."));
    }
}
