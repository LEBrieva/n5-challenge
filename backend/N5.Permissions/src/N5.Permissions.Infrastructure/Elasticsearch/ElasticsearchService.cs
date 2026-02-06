using Nest;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Infrastructure.Elasticsearch;

public class ElasticsearchService : IElasticsearchService
{
    private readonly IElasticClient _client;
    private const string IndexName = "permissions";

    public ElasticsearchService(IElasticClient client)
    {
        _client = client;
    }

    public async Task IndexPermissionAsync(Permission permission)
    {
        var document = new
        {
            permission.Id,
            permission.NombreEmpleado,
            permission.ApellidoEmpleado,
            permission.TipoPermiso,
            permission.FechaPermiso
        };

        await _client.IndexAsync(document, idx => idx.Index(IndexName).Id(permission.Id));
    }
}
