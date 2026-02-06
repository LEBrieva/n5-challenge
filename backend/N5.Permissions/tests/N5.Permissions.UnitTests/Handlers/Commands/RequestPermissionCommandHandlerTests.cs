using FluentAssertions;
using Moq;
using N5.Permissions.Application.Permissions.Commands.RequestPermission;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.UnitTests.Handlers.Commands;

public class RequestPermissionCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPermissionRepository> _mockPermissionRepo;
    private readonly Mock<IElasticsearchService> _mockElasticsearch;
    private readonly Mock<IKafkaProducerService> _mockKafka;
    private readonly RequestPermissionCommandHandler _handler;

    public RequestPermissionCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPermissionRepo = new Mock<IPermissionRepository>();
        _mockElasticsearch = new Mock<IElasticsearchService>();
        _mockKafka = new Mock<IKafkaProducerService>();

        _mockUnitOfWork.Setup(u => u.Permissions).Returns(_mockPermissionRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new RequestPermissionCommandHandler(
            _mockUnitOfWork.Object,
            _mockElasticsearch.Object,
            _mockKafka.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsPermissionId()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            NombreEmpleado = "Juan",
            ApellidoEmpleado = "Pérez",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Now
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAddAsync()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            NombreEmpleado = "María",
            ApellidoEmpleado = "González",
            TipoPermiso = 2,
            FechaPermiso = DateTime.Now
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPermissionRepo.Verify(
            r => r.AddAsync(It.Is<Permission>(p =>
                p.NombreEmpleado == "María" &&
                p.ApellidoEmpleado == "González" &&
                p.TipoPermiso == 2
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            NombreEmpleado = "Pedro",
            ApellidoEmpleado = "López",
            TipoPermiso = 3,
            FechaPermiso = DateTime.Now
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
    public async Task Handle_ValidCommand_CallsElasticsearchIndexing()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            NombreEmpleado = "Ana",
            ApellidoEmpleado = "Martínez",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Now
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockElasticsearch.Verify(
            e => e.IndexPermissionAsync(It.IsAny<Permission>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesKafkaMessage()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            NombreEmpleado = "Carlos",
            ApellidoEmpleado = "Rodríguez",
            TipoPermiso = 4,
            FechaPermiso = DateTime.Now
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockKafka.Verify(
            k => k.PublishOperationAsync("request"),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ExecutesInCorrectOrder()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            NombreEmpleado = "Luis",
            ApellidoEmpleado = "Fernández",
            TipoPermiso = 5,
            FechaPermiso = DateTime.Now
        };

        var callOrder = new List<string>();

        _mockPermissionRepo
            .Setup(r => r.AddAsync(It.IsAny<Permission>()))
            .Callback(() => callOrder.Add("AddAsync"));

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        _mockElasticsearch
            .Setup(e => e.IndexPermissionAsync(It.IsAny<Permission>()))
            .Callback(() => callOrder.Add("Elasticsearch"))
            .Returns(Task.CompletedTask);

        _mockKafka
            .Setup(k => k.PublishOperationAsync("request"))
            .Callback(() => callOrder.Add("Kafka"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().ContainInOrder("AddAsync", "SaveChanges", "Elasticsearch", "Kafka");
    }
}
