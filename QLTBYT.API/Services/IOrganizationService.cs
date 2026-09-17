using QLTBYT.API.Models.DTOs.Organization;

namespace QLTBYT.API.Services;

public interface IOrganizationService
{
    Task<List<OrganizationListDto>> GetOrganizationsAsync();
    Task<OrganizationListDto?> GetOrganizationAsync(int id);
    Task<OrganizationListDto> CreateOrganizationAsync(CreateOrganizationRequest request);
    Task<OrganizationListDto?> UpdateOrganizationAsync(int id, UpdateOrganizationRequest request);
    Task<bool> DeleteOrganizationAsync(int id);
}
