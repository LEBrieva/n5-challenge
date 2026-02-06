using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using N5.Permissions.Domain.Entities;

namespace N5.Permissions.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.NombreEmpleado)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ApellidoEmpleado)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.FechaPermiso)
            .IsRequired();

        builder.HasOne(p => p.PermissionType)
            .WithMany(pt => pt.Permissions)
            .HasForeignKey(p => p.TipoPermiso)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
