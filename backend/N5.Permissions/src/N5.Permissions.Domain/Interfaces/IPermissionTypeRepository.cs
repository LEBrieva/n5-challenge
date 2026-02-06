using N5.Permissions.Domain.Entities;

namespace N5.Permissions.Domain.Interfaces;

public interface IPermissionTypeRepository
{
    Task<PermissionType?> GetByIdAsync(int id);
    Task<IEnumerable<PermissionType>> GetAllAsync();
}
