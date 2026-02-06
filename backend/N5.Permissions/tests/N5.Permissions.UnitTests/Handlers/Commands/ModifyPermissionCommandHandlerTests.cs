using FluentAssertions;
using Moq;
using N5.Permissions.Application.Permissions.Commands.ModifyPermission;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.UnitTests.Handlers.Commands;

public class ModifyPermissionCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPermissionRepository> _mockPermissionRepo;
    private readonly Mock<IElasticsearchService> _mockElasticsearch;
    private readonly Mock<IKafkaProducerService> _mockKafka;
    private readonly ModifyPermissionCommandHandler _handler;

    public ModifyPermissionCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPermissionRepo = new Mock<IPermissionRepository>();
        _mockElasticsearch = new Mock<IElasticsearchService>();
        _mockKafka = new Mock<IKafkaProducerService>();

        _mockUnitOfWork.Setup(u => u.Permissions).Returns(_mockPermissionRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new ModifyPermissionCommandHandler(
            _mockUnitOfWork.Object,
            _mockElasticsearch.Object,
            _mockKafka.Object
        );
    }

    [Fact]
    public async Task Handle_ExistingPermission_ReturnsTrue()
    {
        // Arrange
        var existingPermission = new Permission
        {
            Id = 1,
            NombreEmpleado = "Juan",
            ApellidoEmpleado = "Pérez",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Now
        };

        _mockPermissionRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingPermission);

        var command = new ModifyPermissionCommand
        {
            Id = 1,
            NombreEmpleado = "Juan Carlos",
            ApellidoEmpleado = "Pérez García",
            TipoPermiso = 2,
            FechaPermiso = DateTime.Now.AddDays(1)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistingPermission_ReturnsFalse()
    {
        // Arrange
        _mockPermissionRepo
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Permission?)null);

        var command = new ModifyPermissionCommand
        {
            Id = 999,
            NombreEmpleado = "Test",
            ApellidoEmpleado = "User",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Now
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ExistingPermission_UpdatesAllProperties()
    {
        // Arrange
        var existingPermission = new Permission
        {
            Id = 1,
            NombreEmpleado = "Old Name",
            ApellidoEmpleado = "Old Surname",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Parse("2024-01-01")
        };

        _mockPermissionRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingPermission);

        var command = new ModifyPermissionCommand
        {
            Id = 1,
            NombreEmpleado = "New Name",
            ApellidoEmpleado = "New Surname",
            TipoPermiso = 3,
            FechaPermiso = DateTime.Parse("2024-12-31")
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingPermission.NombreEmpleado.Should().Be("New Name");
        existingPermission.ApellidoEmpleado.Should().Be("New Surname");
        existingPermission.TipoPermiso.Should().Be(3);
        existingPermission.FechaPermiso.Should().Be(DateTime.Parse("2024-12-31"));
    }

    [Fact]
    public async Task Handle_ExistingPermission_CallsRepositoryUpdate()
    {
        // Arrange
        var existingPermission = new Permission
        {
            Id = 1,
            NombreEmpleado = "María",
            ApellidoEmpleado = "González",
            TipoPermiso = 2,
            FechaPermiso = DateTime.Now
        };

        _mockPermissionRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingPermission);

        var command = new ModifyPermissionCommand
        {
            Id = 1,
            NombreEmpleado = "María Isabel",
            ApellidoEmpleado = "González López",
            TipoPermiso = 4,
            FechaPermiso = DateTime.Now.AddDays(5)
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPermissionRepo.Verify(
            r => r.Update(It.Is<Permission>(p => p.Id == 1)),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ExistingPermission_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var existingPermission = new Permission
        {
            Id = 1,
            NombreEmpleado = "Pedro",
            ApellidoEmpleado = "López",
            TipoPermiso = 3,
            FechaPermiso = DateTime.Now
        };

        _mockPermissionRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingPermission);

        var command = new ModifyPermissionCommand
        {
            Id = 1,
            NombreEmpleado = "Pedro José",
            ApellidoEmpleado = "López Ramírez",
            TipoPermiso = 5,
            FechaPermiso = DateTime.Now.AddDays(10)
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ExistingPermission_ReindexesInElasticsearch()
    {
        // Arrange
        var existingPermission = new Permission
        {
            Id = 1,
            NombreEmpleado = "Ana",
            ApellidoEmpleado = "Martínez",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Now
        };

        _mockPermissionRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingPermission);

        var command = new ModifyPermissionCommand
        {
            Id = 1,
            NombreEmpleado = "Ana María",
            ApellidoEmpleado = "Martínez Ruiz",
            TipoPermiso = 2,
            FechaPermiso = DateTime.Now.AddDays(3)
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockElasticsearch.Verify(
            e => e.IndexPermissionAsync(It.Is<Permission>(p => p.Id == 1)),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ExistingPermission_PublishesModifyOperationToKafka()
    {
        // Arrange
        var existingPermission = new Permission
        {
            Id = 1,
            NombreEmpleado = "Carlos",
            ApellidoEmpleado = "Rodríguez",
            TipoPermiso = 4,
            FechaPermiso = DateTime.Now
        };

        _mockPermissionRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingPermission);

        var command = new ModifyPermissionCommand
        {
            Id = 1,
            NombreEmpleado = "Carlos Alberto",
            ApellidoEmpleado = "Rodríguez Sánchez",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Now.AddDays(7)
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockKafka.Verify(
            k => k.PublishOperationAsync("modify"),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_NonExistingPermission_DoesNotCallUpdateOrSave()
    {
        // Arrange
        _mockPermissionRepo
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Permission?)null);

        var command = new ModifyPermissionCommand
        {
            Id = 999,
            NombreEmpleado = "Test",
            ApellidoEmpleado = "User",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Now
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPermissionRepo.Verify(r => r.Update(It.IsAny<Permission>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockElasticsearch.Verify(e => e.IndexPermissionAsync(It.IsAny<Permission>()), Times.Never);
        _mockKafka.Verify(k => k.PublishOperationAsync(It.IsAny<string>()), Times.Never);
    }
}
