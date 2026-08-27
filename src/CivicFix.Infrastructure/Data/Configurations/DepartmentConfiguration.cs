using CivicFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFix.Infrastructure.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(d => d.Code)
            .IsUnique();

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.ContactEmail)
            .HasMaxLength(200);

        builder.Property(d => d.ContactPhone)
            .HasMaxLength(50);

        builder.Property(d => d.HeadOfficerName)
            .HasMaxLength(150);
    }
}
