using MediatR;

namespace N5.Permissions.Application.Permissions.Commands.RequestPermission;

public class RequestPermissionCommand : IRequest<int>
{
    public string NombreEmpleado { get; set; } = string.Empty;
    public string ApellidoEmpleado { get; set; } = string.Empty;
    public int TipoPermiso { get; set; }
    public DateTime FechaPermiso { get; set; }
}
