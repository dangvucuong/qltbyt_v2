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
            .ForMember(d => d.LegalDocumentRef, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

        CreateMap<UpdateDeviceRequest, Device>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.DeviceCode, o => o.Ignore())
            .ForMember(d => d.LegalDocumentRef, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));
    }
}
