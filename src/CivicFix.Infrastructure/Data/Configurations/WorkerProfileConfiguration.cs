using CivicFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFix.Infrastructure.Data.Configurations;

public class WorkerProfileConfiguration : IEntityTypeConfiguration<WorkerProfile>
{
    public void Configure(EntityTypeBuilder<WorkerProfile> builder)
    {
        builder.HasKey(w => w.UserId);

        builder.Property(w => w.UserId)
            .ValueGeneratedNever();

        builder.Property(w => w.Specialization)
            .HasMaxLength(150);

        builder.Property(w => w.AssignedWardOrZone)
            .HasMaxLength(100);

        builder.Property(w => w.Rating)
            .HasPrecision(3, 2);

        builder.HasOne(w => w.User)
            .WithOne(u => u.WorkerProfile)
            .HasForeignKey<WorkerProfile>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
