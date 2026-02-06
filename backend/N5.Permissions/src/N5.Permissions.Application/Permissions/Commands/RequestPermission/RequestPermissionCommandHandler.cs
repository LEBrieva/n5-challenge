using MediatR;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Application.Permissions.Commands.RequestPermission;

public class RequestPermissionCommandHandler
    : IRequestHandler<RequestPermissionCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly IKafkaProducerService _kafkaProducer;

    public RequestPermissionCommandHandler(
        IUnitOfWork unitOfWork,
        IElasticsearchService elasticsearchService,
        IKafkaProducerService kafkaProducer)
    {
        _unitOfWork = unitOfWork;
        _elasticsearchService = elasticsearchService;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<int> Handle(
        RequestPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var permission = new Permission
        {
            NombreEmpleado = request.NombreEmpleado,
            ApellidoEmpleado = request.ApellidoEmpleado,
            TipoPermiso = request.TipoPermiso,
            FechaPermiso = request.FechaPermiso
        };

        await _unitOfWork.Permissions.AddAsync(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _elasticsearchService.IndexPermissionAsync(permission);
        await _kafkaProducer.PublishOperationAsync("request");

        return permission.Id;
    }
}
