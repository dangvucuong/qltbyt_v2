using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Organization;

namespace QLTBYT.API.Services;

public partial class OrganizationService : IOrganizationService
{
    public async Task<OrganizationListDto?> GetOrganizationAsync(int id)
    {
        var o = await _context.Organizations.AsNoTracking()
            .Include(x => x.OrganizationType)
            .FirstOrDefaultAsync(x => x.Id == id);
        return o is null ? null : Map(o);
    }

    public async Task<OrganizationListDto> CreateOrganizationAsync(CreateOrganizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Ten don vi la bat buoc.");

        var entity = new Organization
        {
            Name = request.Name.Trim(),
            TypeId = request.TypeId,
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            TaxCode = request.TaxCode?.Trim()
        };
        _context.Organizations.Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(e => e.OrganizationType).LoadAsync();
        return Map(entity);
    }

    public async Task<OrganizationListDto?> UpdateOrganizationAsync(int id, UpdateOrganizationRequest request)
    {
        var entity = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (entity is null) return null;
        if (request.Name is not null) entity.Name = request.Name.Trim();
        if (request.TypeId.HasValue) entity.TypeId = request.TypeId;
        if (request.Phone is not null) entity.Phone = request.Phone.Trim();
        if (request.Email is not null) entity.Email = request.Email.Trim();
        if (request.Address is not null) entity.Address = request.Address.Trim();
        if (request.TaxCode is not null) entity.TaxCode = request.TaxCode.Trim();
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(e => e.OrganizationType).LoadAsync();
        return Map(entity);
    }

    public async Task<bool> DeleteOrganizationAsync(int id)
    {
        var hasDevices = await _context.Devices.AnyAsync(d => d.OrganizationId == id);
        if (hasDevices)
            throw new InvalidOperationException("Khong xoa don vi con thiet bi.");

        var entity = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (entity is null) return false;
        _context.Organizations.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    private static OrganizationListDto Map(Organization o) => new()
    {
        Id = o.Id,
        Name = o.Name,
        TypeId = o.TypeId,
        TypeName = o.OrganizationType?.Name,
        Phone = o.Phone,
        Email = o.Email,
        Address = o.Address,
        TaxCode = o.TaxCode
    };
}
