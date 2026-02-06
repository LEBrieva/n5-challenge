using MediatR;
using N5.Permissions.Application.DTOs;

namespace N5.Permissions.Application.Permissions.Queries.GetPermissions;

public class GetPermissionsQuery : IRequest<IEnumerable<PermissionDto>>
{
}
