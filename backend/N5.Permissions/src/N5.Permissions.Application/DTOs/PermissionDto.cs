namespace N5.Permissions.Application.DTOs;

public class PermissionDto
{
    public int Id { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string ApellidoEmpleado { get; set; } = string.Empty;
    public int TipoPermiso { get; set; }
    public string TipoPermisoDescripcion { get; set; } = string.Empty;
    public DateTime FechaPermiso { get; set; }
}
