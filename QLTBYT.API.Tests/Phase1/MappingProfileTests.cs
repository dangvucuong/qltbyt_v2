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
            DeviceType = new DeviceType { Id = 1, Name = "Máy thở" },
            Organization = new Organization { Id = 2, Name = "Khoa Nội" },
            DeviceStatus = new DeviceStatus { Id = 1, Name = "Đang hoạt động" }
        };

        var dto = _mapper.Map<DeviceListDto>(device);

        Assert.Equal("TB-TEST", dto.DeviceCode);
        Assert.Equal("Máy thở", dto.TypeName);
        Assert.Equal("Khoa Nội", dto.OrganizationName);
        Assert.Equal("Đang hoạt động", dto.StatusName);
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
