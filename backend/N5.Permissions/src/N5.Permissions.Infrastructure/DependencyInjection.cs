using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Elasticsearch;
using N5.Permissions.Infrastructure.Kafka;
using N5.Permissions.Infrastructure.Persistence;
using N5.Permissions.Infrastructure.Persistence.Repositories;
using Nest;

namespace N5.Permissions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Solo registrar DbContext si no está ya registrado (permite a los tests usar InMemory)
        var dbContextOptionsDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

        if (dbContextOptionsDescriptor == null)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("SqlServer")));
        }

        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionTypeRepository, PermissionTypeRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var elasticsearchUri = configuration["Elasticsearch:Uri"];
        var settings = new ConnectionSettings(new Uri(elasticsearchUri!))
            .DefaultIndex(configuration["Elasticsearch:IndexName"]);
        services.AddSingleton<IElasticClient>(new ElasticClient(settings));
        services.AddScoped<IElasticsearchService, ElasticsearchService>();

        var kafkaConfig = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };
        services.AddSingleton<IProducer<string, string>>(
            new ProducerBuilder<string, string>(kafkaConfig).Build());
        services.AddScoped<IKafkaProducerService, KafkaProducerService>();

        return services;
    }
}
