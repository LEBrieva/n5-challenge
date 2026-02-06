using MediatR;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Application.Permissions.Commands.ModifyPermission;

public class ModifyPermissionCommandHandler
    : IRequestHandler<ModifyPermissionCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly IKafkaProducerService _kafkaProducer;

    public ModifyPermissionCommandHandler(
        IUnitOfWork unitOfWork,
        IElasticsearchService elasticsearchService,
        IKafkaProducerService kafkaProducer)
    {
        _unitOfWork = unitOfWork;
        _elasticsearchService = elasticsearchService;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<bool> Handle(
        ModifyPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var permission = await _unitOfWork.Permissions.GetByIdAsync(request.Id);

        if (permission == null)
            return false;

        permission.NombreEmpleado = request.NombreEmpleado;
        permission.ApellidoEmpleado = request.ApellidoEmpleado;
        permission.TipoPermiso = request.TipoPermiso;
        permission.FechaPermiso = request.FechaPermiso;

        _unitOfWork.Permissions.Update(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _elasticsearchService.IndexPermissionAsync(permission);
        await _kafkaProducer.PublishOperationAsync("modify");

        return true;
    }
}
