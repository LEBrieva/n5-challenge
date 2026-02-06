using FluentAssertions;
using Moq;
using N5.Permissions.Application.Permissions.Queries.GetPermissions;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.UnitTests.Handlers.Queries;

public class GetPermissionsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPermissionRepository> _mockPermissionRepo;
    private readonly Mock<IKafkaProducerService> _mockKafka;
    private readonly GetPermissionsQueryHandler _handler;

    public GetPermissionsQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPermissionRepo = new Mock<IPermissionRepository>();
        _mockKafka = new Mock<IKafkaProducerService>();

        _mockUnitOfWork.Setup(u => u.Permissions).Returns(_mockPermissionRepo.Object);

        _handler = new GetPermissionsQueryHandler(
            _mockUnitOfWork.Object,
            _mockKafka.Object
        );
    }

    [Fact]
    public async Task Handle_WhenNoPermissions_ReturnsEmptyList()
    {
        // Arrange
        _mockPermissionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Permission>());

        var query = new GetPermissionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPermissionsExist_ReturnsPermissionDtos()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new Permission
            {
                Id = 1,
                NombreEmpleado = "Juan",
                ApellidoEmpleado = "Pérez",
                TipoPermiso = 1,
                FechaPermiso = DateTime.Parse("2024-01-15"),
                PermissionType = new PermissionType { Id = 1, Descripcion = "Vacaciones" }
            },
            new Permission
            {
                Id = 2,
                NombreEmpleado = "María",
                ApellidoEmpleado = "González",
                TipoPermiso = 2,
                FechaPermiso = DateTime.Parse("2024-02-20"),
                PermissionType = new PermissionType { Id = 2, Descripcion = "Licencia médica" }
            }
        };

        _mockPermissionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(permissions);

        var query = new GetPermissionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenPermissionsExist_MapsPropertiesCorrectly()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new Permission
            {
                Id = 1,
                NombreEmpleado = "Pedro",
                ApellidoEmpleado = "López",
                TipoPermiso = 3,
                FechaPermiso = DateTime.Parse("2024-03-25"),
                PermissionType = new PermissionType { Id = 3, Descripcion = "Permiso personal" }
            }
        };

        _mockPermissionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(permissions);

        var query = new GetPermissionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.Id.Should().Be(1);
        dto.NombreEmpleado.Should().Be("Pedro");
        dto.ApellidoEmpleado.Should().Be("López");
        dto.TipoPermiso.Should().Be(3);
        dto.TipoPermisoDescripcion.Should().Be("Permiso personal");
        dto.FechaPermiso.Should().Be(DateTime.Parse("2024-03-25"));
    }

    [Fact]
    public async Task Handle_WhenPermissionTypeIsNull_UsesEmptyDescription()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new Permission
            {
                Id = 1,
                NombreEmpleado = "Ana",
                ApellidoEmpleado = "Martínez",
                TipoPermiso = 1,
                FechaPermiso = DateTime.Parse("2024-04-10"),
                PermissionType = null
            }
        };

        _mockPermissionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(permissions);

        var query = new GetPermissionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.TipoPermisoDescripcion.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Handle_WhenMultiplePermissions_PreservesOrder()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new Permission
            {
                Id = 1,
                NombreEmpleado = "Carlos",
                ApellidoEmpleado = "Rodríguez",
                TipoPermiso = 4,
                FechaPermiso = DateTime.Parse("2024-05-01"),
                PermissionType = new PermissionType { Id = 4, Descripcion = "Día libre" }
            },
            new Permission
            {
                Id = 2,
                NombreEmpleado = "Luis",
                ApellidoEmpleado = "Fernández",
                TipoPermiso = 5,
                FechaPermiso = DateTime.Parse("2024-06-15"),
                PermissionType = new PermissionType { Id = 5, Descripcion = "Trabajo remoto" }
            },
            new Permission
            {
                Id = 3,
                NombreEmpleado = "Elena",
                ApellidoEmpleado = "Ramírez",
                TipoPermiso = 1,
                FechaPermiso = DateTime.Parse("2024-07-20"),
                PermissionType = new PermissionType { Id = 1, Descripcion = "Vacaciones" }
            }
        };

        _mockPermissionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(permissions);

        var query = new GetPermissionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.ElementAt(0).Id.Should().Be(1);
        result.ElementAt(1).Id.Should().Be(2);
        result.ElementAt(2).Id.Should().Be(3);
    }

    [Fact]
    public async Task Handle_CallsRepositoryGetAllAsync()
    {
        // Arrange
        _mockPermissionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Permission>());

        var query = new GetPermissionsQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockPermissionRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PropagatesCancellation()
    {
        // Arrange
        _mockPermissionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Permission>());

        var query = new GetPermissionsQuery();
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await _handler.Handle(query, cancellationTokenSource.Token);

        // Assert
        _mockPermissionRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Always_PublishesGetOperationToKafka()
    {
        // Arrange
        _mockPermissionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Permission>());

        var query = new GetPermissionsQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockKafka.Verify(
            k => k.PublishOperationAsync("get"),
            Times.Once
        );
    }
}
