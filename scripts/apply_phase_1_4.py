# -*- coding: utf-8 -*-
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
API = ROOT / "QLTBYT.API"
TESTS = ROOT / "QLTBYT.API.Tests" / "Phase1"

def w(rel: str, content: str) -> None:
    p = ROOT / rel.replace("/", "\\") if "\\" not in rel else Path(rel)
    if not str(p).startswith(str(ROOT)):
        p = ROOT / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content.strip() + "\n", encoding="utf-8", newline="\n")
    print("written", p.relative_to(ROOT))

w("QLTBYT.API/Models/Common/PagedResult.cs", """
namespace QLTBYT.API.Models.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
""")

w("QLTBYT.API/Models/DTOs/Device/DeviceListDto.cs", """
namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceListDto
{
    public int Id { get; set; }
    public string? DeviceCode { get; set; }
    public string? Name { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
}
""")

w("QLTBYT.API/Models/DTOs/Device/DeviceFileDto.cs", """
namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceFileDto
{
    public int Id { get; set; }
    public string? Path { get; set; }
    public string? Description { get; set; }
    public DateTime? FileDate { get; set; }
}
""")

w("QLTBYT.API/Models/DTOs/Device/DeviceCodeDto.cs", """
namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceCodeDto
{
    public int Id { get; set; }
    public string? QrCode { get; set; }
    public string? Nfc { get; set; }
}
""")

w("QLTBYT.API/Models/DTOs/Device/DeviceDetailDto.cs", """
namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceDetailDto
{
    public int Id { get; set; }
    public string? DeviceCode { get; set; }
    public string? TechnicalCode { get; set; }
    public string? IdentityCode { get; set; }
    public string? Name { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public string? Manufacturer { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? ParentDeviceCode { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public string? Note { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DeviceFileDto> Files { get; set; } = [];
    public List<DeviceCodeDto> Codes { get; set; } = [];
}
""")

w("QLTBYT.API/Models/DTOs/Device/DeviceQueryParams.cs", """
namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
}
""")

w("QLTBYT.API/Models/DTOs/Device/CreateDeviceRequest.cs", """
namespace QLTBYT.API.Models.DTOs.Device;

public class CreateDeviceRequest
{
    public string DeviceCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public int OrganizationId { get; set; }
    public int StatusId { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public string? Manufacturer { get; set; }
    public string? CountryOfOrigin { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public string? Note { get; set; }
    public string? ParentDeviceCode { get; set; }
}
""")

w("QLTBYT.API/Models/DTOs/Device/UpdateDeviceRequest.cs", """
namespace QLTBYT.API.Models.DTOs.Device;

public class UpdateDeviceRequest
{
    public string Name { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public int OrganizationId { get; set; }
    public int StatusId { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public string? Manufacturer { get; set; }
    public string? CountryOfOrigin { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public string? Note { get; set; }
    public string? ParentDeviceCode { get; set; }
}
""")

w("QLTBYT.API/Mappings/MappingProfile.cs", """
using AutoMapper;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Device, DeviceListDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.DeviceType != null ? s.DeviceType.Name : null))
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Organization != null ? s.Organization.Name : null))
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.DeviceStatus != null ? s.DeviceStatus.Name : null));

        CreateMap<Device, DeviceDetailDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.DeviceType != null ? s.DeviceType.Name : null))
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Organization != null ? s.Organization.Name : null))
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.DeviceStatus != null ? s.DeviceStatus.Name : null));

        CreateMap<DeviceFile, DeviceFileDto>();
        CreateMap<DeviceCode, DeviceCodeDto>();

        CreateMap<CreateDeviceRequest, Device>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

        CreateMap<UpdateDeviceRequest, Device>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.DeviceCode, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));
    }
}
""")

w("QLTBYT.API/Validators/LoginRequestValidator.cs", """
using FluentValidation;
using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Ten dang nhap khong duoc de trống.")
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mat khau khong duoc de trống.")
            .MinimumLength(6).WithMessage("Mat khau phai co it nhat 6 ky tu.");
    }
}
""")

w("QLTBYT.API/Validators/CreateDeviceRequestValidator.cs", """
using FluentValidation;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Validators;

public class CreateDeviceRequestValidator : AbstractValidator<CreateDeviceRequest>
{
    public CreateDeviceRequestValidator()
    {
        RuleFor(x => x.DeviceCode)
            .NotEmpty().WithMessage("Ma thiet bi khong duoc de trống.")
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ten thiet bi khong duoc de trống.")
            .MaximumLength(500);

        RuleFor(x => x.TypeId).GreaterThan(0).WithMessage("Loai thiet bi khong hop le.");
        RuleFor(x => x.OrganizationId).GreaterThan(0).WithMessage("Don vi su dung khong hop le.");
        RuleFor(x => x.StatusId).GreaterThan(0).WithMessage("Trang thai khong hop le.");
    }
}
""")

w("QLTBYT.API/Validators/UpdateDeviceRequestValidator.cs", """
using FluentValidation;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Validators;

public class UpdateDeviceRequestValidator : AbstractValidator<UpdateDeviceRequest>
{
    public UpdateDeviceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ten thiet bi khong duoc de trống.")
            .MaximumLength(500);

        RuleFor(x => x.TypeId).GreaterThan(0).WithMessage("Loai thiet bi khong hop le.");
        RuleFor(x => x.OrganizationId).GreaterThan(0).WithMessage("Don vi su dung khong hop le.");
        RuleFor(x => x.StatusId).GreaterThan(0).WithMessage("Trang thai khong hop le.");
    }
}
""")

w("QLTBYT.API/Services/IDeviceService.cs", """
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Services;

public interface IDeviceService
{
    Task<PagedResult<DeviceListDto>> GetDevicesAsync(DeviceQueryParams query);
    Task<DeviceDetailDto?> GetDeviceByIdAsync(int id);
    Task<DeviceDetailDto> CreateDeviceAsync(CreateDeviceRequest request);
}
""")

w("QLTBYT.API/Services/DeviceService.cs", """
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Services;

public class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public DeviceService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<DeviceListDto>> GetDevicesAsync(DeviceQueryParams query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var queryable = _context.Devices
            .AsNoTracking()
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceStatus)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            queryable = queryable.Where(d =>
                (d.Name != null && d.Name.Contains(term)) ||
                (d.DeviceCode != null && d.DeviceCode.Contains(term)));
        }

        var total = await queryable.CountAsync();
        var items = await queryable
            .OrderBy(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<DeviceListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<DeviceListDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DeviceDetailDto?> GetDeviceByIdAsync(int id)
    {
        var device = await _context.Devices
            .AsNoTracking()
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceStatus)
            .Include(d => d.Files)
            .Include(d => d.Codes)
            .FirstOrDefaultAsync(d => d.Id == id);

        return device is null ? null : _mapper.Map<DeviceDetailDto>(device);
    }

    public async Task<DeviceDetailDto> CreateDeviceAsync(CreateDeviceRequest request)
    {
        var exists = await _context.Devices.AnyAsync(d => d.DeviceCode == request.DeviceCode);
        if (exists)
        {
            throw new InvalidOperationException("Ma thiet bi da ton tai.");
        }

        var entity = _mapper.Map<Device>(request);
        _context.Devices.Add(entity);
        await _context.SaveChangesAsync();

        return await GetDeviceByIdAsync(entity.Id)
            ?? throw new InvalidOperationException("Khong the doc thiet bi sau khi tao.");
    }
}
""")

w("QLTBYT.API/Controllers/DevicesController.cs", """
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize(Policy = "AllUsers")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DeviceListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<DeviceListDto>>>> GetDevices(
        [FromQuery] DeviceQueryParams queryParams)
    {
        var result = await _deviceService.GetDevicesAsync(queryParams);
        return Ok(ApiResponse<PagedResult<DeviceListDto>>.CreateSuccess(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DeviceDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DeviceDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> GetDevice(int id)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device is null)
        {
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Khong tim thay thiet bi."));
        }

        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(device));
    }

    [HttpPost]
    [Authorize(Policy = "TechnicianAndAbove")]
    [ProducesResponseType(typeof(ApiResponse<DeviceDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DeviceDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> CreateDevice(
        [FromBody] CreateDeviceRequest request)
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
}
""")

w("QLTBYT.API.Tests/Phase1/PagedResultTests.cs", """
using QLTBYT.API.Models.Common;

namespace QLTBYT.API.Tests.Phase1;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_Rounds_Up()
    {
        var paged = new PagedResult<int>
        {
            TotalCount = 21,
            Page = 1,
            PageSize = 20,
            Items = []
        };

        Assert.Equal(2, paged.TotalPages);
    }
}
""")

w("QLTBYT.API.Tests/Phase1/MappingProfileTests.cs", """
using AutoMapper;
using QLTBYT.API.Mappings;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Tests.Phase1;

public class MappingProfileTests
{
  private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

    [Fact]
    public void Device_Maps_To_ListDto_With_DisplayNames()
    {
        var device = new Device
        {
            Id = 1,
            DeviceCode = "TB-TEST",
            Name = "Thiet bi test",
            TypeId = 1,
            OrganizationId = 2,
            StatusId = 1,
            DeviceType = new DeviceType { Id = 1, Name = "May tho" },
            Organization = new Organization { Id = 2, Name = "Khoa Noi" },
            DeviceStatus = new DeviceStatus { Id = 1, Name = "Dang hoat dong" }
        };

        var dto = _mapper.Map<DeviceListDto>(device);

        Assert.Equal("TB-TEST", dto.DeviceCode);
        Assert.Equal("May tho", dto.TypeName);
        Assert.Equal("Khoa Noi", dto.OrganizationName);
        Assert.Equal("Dang hoat dong", dto.StatusName);
    }

    [Fact]
    public void CreateDeviceRequest_Maps_To_Device()
    {
        var request = new CreateDeviceRequest
        {
            DeviceCode = "TB-NEW-999",
            Name = "Thiet bi moi",
            TypeId = 1,
            OrganizationId = 1,
            StatusId = 1
        };

        var entity = _mapper.Map<Device>(request);

        Assert.Equal("TB-NEW-999", entity.DeviceCode);
        Assert.Equal("Thiet bi moi", entity.Name);
        Assert.NotNull(entity.UpdatedAt);
    }
}
""")

w("QLTBYT.API.Tests/Phase1/CreateDeviceRequestValidatorTests.cs", """
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Validators;

namespace QLTBYT.API.Tests.Phase1;

public class CreateDeviceRequestValidatorTests
{
    private readonly CreateDeviceRequestValidator _validator = new();

    [Fact]
    public void Empty_DeviceCode_Fails()
    {
        var result = _validator.Validate(new CreateDeviceRequest
        {
            DeviceCode = "",
            Name = "Ten",
            TypeId = 1,
            OrganizationId = 1,
            StatusId = 1
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDeviceRequest.DeviceCode));
    }
}
""")

w("QLTBYT.API.Tests/Phase1/DevicesControllerIntegrationTests.cs", """
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase1;

[Trait("Category", "Database")]
public class DevicesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DevicesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionStrings.Resolve());
            builder.UseSetting("JwtSettings:SecretKey", "QLTBYT-TEST-Secret-Key-At-Least-32-Characters");
            builder.UseSetting("JwtSettings:Issuer", "QLTBYT-API");
            builder.UseSetting("JwtSettings:Audience", "QLTBYT-Frontend");
            builder.UseSetting(Microsoft.AspNetCore.Hosting.WebHostDefaults.EnvironmentKey, "Development");
        });
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username = "admin")
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = username,
            Password = "Sample@123"
        });
        var body = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Data!.Token);
        return client;
    }

    [Fact]
    public async Task GetDevices_Returns_PagedResult_With_TotalCount()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/devices?page=1&pageSize=5&search=DEMO");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.TotalCount >= 3);
        Assert.NotEmpty(body.Data.Items);
        Assert.Equal(1, body.Data.Page);
        Assert.Equal(5, body.Data.PageSize);
    }

    [Fact]
    public async Task PostDevice_Invalid_Returns_400_With_Errors()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/devices", new CreateDeviceRequest
        {
            DeviceCode = "",
            Name = "",
            TypeId = 0,
            OrganizationId = 0,
            StatusId = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DeviceDetailDto>>();
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.NotNull(body.Errors);
        Assert.NotEmpty(body.Errors);
    }

    [Fact]
    public async Task PostDevice_Valid_Creates_Sample_Row()
    {
        var client = await CreateAuthenticatedClientAsync();
        var code = "TB-PHASE14-" + Guid.NewGuid().ToString("N")[..6];
        var response = await client.PostAsJsonAsync("/api/devices", new CreateDeviceRequest
        {
            DeviceCode = code,
            Name = "Thiet bi test Phase 1.4",
            TypeId = 1,
            OrganizationId = 1,
            StatusId = 1,
            Manufacturer = "Demo"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DeviceDetailDto>>();
        Assert.NotNull(body?.Data);
        Assert.Equal(code, body.Data.DeviceCode);
    }
}
""")

print("done")
