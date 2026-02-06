using Microsoft.EntityFrameworkCore;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Infrastructure.Persistence.Repositories;

public class PermissionTypeRepository : IPermissionTypeRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PermissionType?> GetByIdAsync(int id)
    {
        return await _context.PermissionTypes.FindAsync(id);
    }

    public async Task<IEnumerable<PermissionType>> GetAllAsync()
    {
        return await _context.PermissionTypes.ToListAsync();
    }
}
