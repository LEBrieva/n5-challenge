using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using N5.Permissions.Application.DTOs;
using N5.Permissions.Application.Permissions.Commands.ModifyPermission;
using N5.Permissions.Application.Permissions.Commands.RequestPermission;

namespace N5.Permissions.IntegrationTests;

public class PermissionsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PermissionsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPermissions_ReturnsOkWithList()
    {
        // Act
        var response = await _client.GetAsync("/api/permissions");
        var permissions = await response.Content.ReadFromJsonAsync<List<PermissionDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        permissions.Should().NotBeNull();
        // Note: May contain data from other tests running in parallel
    }

    [Fact]
    public async Task RequestPermission_ValidCommand_ReturnsOkWithId()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            NombreEmpleado = "Juan",
            ApellidoEmpleado = "Pérez",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Parse("2024-02-15")
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/permissions", command);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Should().ContainKey("id");
        result!["id"].Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RequestPermission_ValidCommand_AddsPermissionToDatabase()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            NombreEmpleado = "María",
            ApellidoEmpleado = "González",
            TipoPermiso = 2,
            FechaPermiso = DateTime.Parse("2024-03-20")
        };

        // Act
        await _client.PostAsJsonAsync("/api/permissions", command);
        var getResponse = await _client.GetAsync("/api/permissions");
        var permissions = await getResponse.Content.ReadFromJsonAsync<List<PermissionDto>>();

        // Assert
        permissions.Should().NotBeNull();
        permissions.Should().Contain(p =>
            p.NombreEmpleado == "María" &&
            p.ApellidoEmpleado == "González" &&
            p.TipoPermiso == 2 &&
            p.TipoPermisoDescripcion == "Licencia médica"
        );
    }

    [Fact]
    public async Task ModifyPermission_ExistingPermission_ReturnsOk()
    {
        // Arrange - Create a permission first
        var createCommand = new RequestPermissionCommand
        {
            NombreEmpleado = "Pedro",
            ApellidoEmpleado = "López",
            TipoPermiso = 3,
            FechaPermiso = DateTime.Parse("2024-04-10")
        };
        var createResponse = await _client.PostAsJsonAsync("/api/permissions", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        var permissionId = createResult!["id"];

        var modifyCommand = new ModifyPermissionCommand
        {
            NombreEmpleado = "Pedro José",
            ApellidoEmpleado = "López Ramírez",
            TipoPermiso = 4,
            FechaPermiso = DateTime.Parse("2024-05-15")
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/permissions/{permissionId}", modifyCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ModifyPermission_NonExistingPermission_ReturnsNotFound()
    {
        // Arrange
        var modifyCommand = new ModifyPermissionCommand
        {
            NombreEmpleado = "Test",
            ApellidoEmpleado = "User",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Parse("2024-06-20")
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/permissions/9999", modifyCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ModifyPermission_ExistingPermission_UpdatesAllFields()
    {
        // Arrange - Create a permission first
        var createCommand = new RequestPermissionCommand
        {
            NombreEmpleado = "Ana",
            ApellidoEmpleado = "Martínez",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Parse("2024-07-01")
        };
        var createResponse = await _client.PostAsJsonAsync("/api/permissions", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        var permissionId = createResult!["id"];

        var modifyCommand = new ModifyPermissionCommand
        {
            NombreEmpleado = "Ana María",
            ApellidoEmpleado = "Martínez Ruiz",
            TipoPermiso = 5,
            FechaPermiso = DateTime.Parse("2024-08-10")
        };

        // Act
        await _client.PutAsJsonAsync($"/api/permissions/{permissionId}", modifyCommand);
        var getResponse = await _client.GetAsync("/api/permissions");
        var permissions = await getResponse.Content.ReadFromJsonAsync<List<PermissionDto>>();

        // Assert
        var updatedPermission = permissions?.FirstOrDefault(p => p.Id == permissionId);
        updatedPermission.Should().NotBeNull();
        updatedPermission!.NombreEmpleado.Should().Be("Ana María");
        updatedPermission.ApellidoEmpleado.Should().Be("Martínez Ruiz");
        updatedPermission.TipoPermiso.Should().Be(5);
        updatedPermission.TipoPermisoDescripcion.Should().Be("Trabajo remoto");
        updatedPermission.FechaPermiso.Should().Be(DateTime.Parse("2024-08-10"));
    }

    [Fact]
    public async Task GetPermissions_AfterMultipleInserts_ReturnsAllPermissions()
    {
        // Arrange - Create multiple permissions
        var permissions = new[]
        {
            new RequestPermissionCommand
            {
                NombreEmpleado = "Carlos",
                ApellidoEmpleado = "Rodríguez",
                TipoPermiso = 1,
                FechaPermiso = DateTime.Parse("2024-09-01")
            },
            new RequestPermissionCommand
            {
                NombreEmpleado = "Luis",
                ApellidoEmpleado = "Fernández",
                TipoPermiso = 2,
                FechaPermiso = DateTime.Parse("2024-09-05")
            },
            new RequestPermissionCommand
            {
                NombreEmpleado = "Elena",
                ApellidoEmpleado = "Ramírez",
                TipoPermiso = 3,
                FechaPermiso = DateTime.Parse("2024-09-10")
            }
        };

        foreach (var permission in permissions)
        {
            await _client.PostAsJsonAsync("/api/permissions", permission);
        }

        // Act
        var response = await _client.GetAsync("/api/permissions");
        var result = await response.Content.ReadFromJsonAsync<List<PermissionDto>>();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(3);
        result.Should().Contain(p => p.NombreEmpleado == "Carlos" && p.ApellidoEmpleado == "Rodríguez");
        result.Should().Contain(p => p.NombreEmpleado == "Luis" && p.ApellidoEmpleado == "Fernández");
        result.Should().Contain(p => p.NombreEmpleado == "Elena" && p.ApellidoEmpleado == "Ramírez");
    }

    [Fact]
    public async Task FullWorkflow_CreateModifyGet_WorksCorrectly()
    {
        // Arrange & Act - Create
        var createCommand = new RequestPermissionCommand
        {
            NombreEmpleado = "Workflow",
            ApellidoEmpleado = "Test",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Parse("2024-10-01")
        };
        var createResponse = await _client.PostAsJsonAsync("/api/permissions", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        var permissionId = createResult!["id"];

        // Act - Modify
        var modifyCommand = new ModifyPermissionCommand
        {
            NombreEmpleado = "Modified",
            ApellidoEmpleado = "Workflow",
            TipoPermiso = 3,
            FechaPermiso = DateTime.Parse("2024-11-01")
        };
        await _client.PutAsJsonAsync($"/api/permissions/{permissionId}", modifyCommand);

        // Act - Get
        var getResponse = await _client.GetAsync("/api/permissions");
        var permissions = await getResponse.Content.ReadFromJsonAsync<List<PermissionDto>>();

        // Assert
        var permission = permissions?.FirstOrDefault(p => p.Id == permissionId);
        permission.Should().NotBeNull();
        permission!.NombreEmpleado.Should().Be("Modified");
        permission.ApellidoEmpleado.Should().Be("Workflow");
        permission.TipoPermiso.Should().Be(3);
        permission.TipoPermisoDescripcion.Should().Be("Permiso personal");
    }
}
