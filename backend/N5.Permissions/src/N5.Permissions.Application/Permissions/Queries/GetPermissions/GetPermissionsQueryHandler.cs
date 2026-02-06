using MediatR;
using N5.Permissions.Application.DTOs;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Application.Permissions.Queries.GetPermissions;

public class GetPermissionsQueryHandler
    : IRequestHandler<GetPermissionsQuery, IEnumerable<PermissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaProducerService _kafkaProducer;

    public GetPermissionsQueryHandler(
        IUnitOfWork unitOfWork,
        IKafkaProducerService kafkaProducer)
    {
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<IEnumerable<PermissionDto>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _unitOfWork.Permissions.GetAllAsync();

        await _kafkaProducer.PublishOperationAsync("get");

        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            NombreEmpleado = p.NombreEmpleado,
            ApellidoEmpleado = p.ApellidoEmpleado,
            TipoPermiso = p.TipoPermiso,
            TipoPermisoDescripcion = p.PermissionType?.Descripcion ?? string.Empty,
            FechaPermiso = p.FechaPermiso
        });
    }
}
