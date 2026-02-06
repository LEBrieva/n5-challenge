using N5.Permissions.Domain.Entities;

namespace N5.Permissions.Domain.Interfaces;

public interface IElasticsearchService
{
    Task IndexPermissionAsync(Permission permission);
}
