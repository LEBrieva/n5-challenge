namespace N5.Permissions.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IPermissionRepository Permissions { get; }
    IPermissionTypeRepository PermissionTypes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
