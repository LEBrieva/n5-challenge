using N5.Permissions.Domain.Entities;

namespace N5.Permissions.Domain.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(int id);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task AddAsync(Permission permission);
    void Update(Permission permission);
}
