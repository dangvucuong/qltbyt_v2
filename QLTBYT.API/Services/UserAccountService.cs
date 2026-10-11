using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.User;

namespace QLTBYT.API.Services;

public interface IUserAccountService
{
    Task<List<UserListDto>> GetUsersAsync();
    Task<UserListDto?> GetUserAsync(int id);
    Task<UserListDto> CreateUserAsync(CreateUserRequest request);
    Task<UserListDto?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
}

public class UserAccountService : IUserAccountService
{
    private readonly AppDbContext _context;
    private readonly ISystemAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public UserAccountService(AppDbContext context, ISystemAuditService audit, ICurrentUserService currentUser)
    {
        _context = context;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<List<UserListDto>> GetUsersAsync()
    {
        return await _context.UserAccounts
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => Map(u))
            .ToListAsync();
    }

    public async Task<UserListDto?> GetUserAsync(int id)
    {
        var u = await _context.UserAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return u is null ? null : Map(u);
    }

    public async Task<UserListDto> CreateUserAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Tên đăng nhập và mật khẩu là bắt buộc.");

        var exists = await _context.UserAccounts.AnyAsync(u => u.Username == request.Username.Trim());
        if (exists)
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");

        var entity = new UserAccount
        {
            Username = request.Username.Trim(),
            Password = PasswordHasher.Hash(request.Password),
            FullName = request.FullName?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            AccountType = request.AccountType,
            OrganizationIds = request.OrganizationIds?.Trim(),
            Admin = request.AccountType == 1 ? 1 : 0
        };
        _context.UserAccounts.Add(entity);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Tạo tài khoản {entity.Username}");
        return Map(entity);
    }

    public async Task<UserListDto?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var entity = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == id);
        if (entity is null)
            return null;

        if (request.FullName is not null)
            entity.FullName = request.FullName.Trim();
        if (request.Phone is not null)
            entity.Phone = request.Phone.Trim();
        if (request.Email is not null)
            entity.Email = request.Email.Trim();
        if (request.AccountType.HasValue)
        {
            entity.AccountType = request.AccountType.Value;
            entity.Admin = request.AccountType.Value == 1 ? 1 : 0;
        }
        if (request.OrganizationIds is not null)
            entity.OrganizationIds = request.OrganizationIds.Trim();
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
            entity.Password = PasswordHasher.Hash(request.NewPassword);

        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Cập nhật tài khoản ID={id}");
        return Map(entity);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        if (_currentUser.UserId == id)
            throw new InvalidOperationException("Không thể xóa tài khoản đang đăng nhập.");

        var entity = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == id);
        if (entity is null)
            return false;

        _context.UserAccounts.Remove(entity);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Xóa tài khoản ID={id}");
        return true;
    }

    private static UserListDto Map(UserAccount u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        Phone = u.Phone,
        Email = u.Email,
        AccountType = u.AccountType,
        Role = RoleMapper.ResolveRole(u),
        OrganizationIds = u.OrganizationIds
    };
}

