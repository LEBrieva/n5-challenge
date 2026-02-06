using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Persistence;

namespace N5.Permissions.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
        "Server=localhost,1433;Database=N5PermissionsDb_Test;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the production DbContext
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions>();

            // Add DbContext with test database connection
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(TestConnectionString);
            });

            // Replace external services with mocks
            services.RemoveAll<IElasticsearchService>();
            services.AddScoped<IElasticsearchService>(sp => Mock.Of<IElasticsearchService>());

            services.RemoveAll<IKafkaProducerService>();
            services.AddScoped<IKafkaProducerService>(sp => Mock.Of<IKafkaProducerService>());
        });

        // Initialize test database
        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Drop and recreate database for clean state
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Seed test data
            SeedTestData(db);
        });
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Seed permission types (IDs will be auto-generated)
        context.PermissionTypes.AddRange(
            new PermissionType { Descripcion = "Vacaciones" },
            new PermissionType { Descripcion = "Licencia médica" },
            new PermissionType { Descripcion = "Permiso personal" },
            new PermissionType { Descripcion = "Día libre" },
            new PermissionType { Descripcion = "Trabajo remoto" }
        );

        context.SaveChanges();
    }
}
