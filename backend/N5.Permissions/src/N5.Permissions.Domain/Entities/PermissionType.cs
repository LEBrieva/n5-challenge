using System.Text.Json.Serialization;

namespace N5.Permissions.Domain.Entities;

public class PermissionType
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
