using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IPermissionRepository Permissions { get; }
    public IPermissionTypeRepository PermissionTypes { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        IPermissionRepository permissionRepository,
        IPermissionTypeRepository permissionTypeRepository)
    {
        _context = context;
        Permissions = permissionRepository;
        PermissionTypes = permissionTypeRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
