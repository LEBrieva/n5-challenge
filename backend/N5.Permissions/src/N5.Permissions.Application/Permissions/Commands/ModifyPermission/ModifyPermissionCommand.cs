using MediatR;

namespace N5.Permissions.Application.Permissions.Commands.ModifyPermission;

public class ModifyPermissionCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string ApellidoEmpleado { get; set; } = string.Empty;
    public int TipoPermiso { get; set; }
    public DateTime FechaPermiso { get; set; }
}
