using QLTBYT.API.Models.DTOs.Master;

namespace QLTBYT.API.Services;

public interface IMasterDataService
{
    Task<List<MasterItemDto>> GetDeviceTypesAsync();
    Task<List<MasterItemDto>> GetDeviceStatusesAsync();
    Task<List<MasterItemDto>> GetOrganizationTypesAsync();
    Task<List<MasterItemDto>> GetCountriesAsync();
    Task<List<MasterItemDto>> GetTaskTypesAsync();
    Task<List<MasterItemDto>> GetEventKeywordsAsync();
    Task<List<MasterItemDto>> GetPersonnelTypesAsync();
    Task<List<MasterItemDto>> GetDocumentTypesAsync();
    Task<List<BiddingPackageLookupDto>> GetBiddingPackagesAsync();
}
