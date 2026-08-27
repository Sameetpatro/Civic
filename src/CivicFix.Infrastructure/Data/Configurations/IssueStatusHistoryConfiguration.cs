using CivicFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFix.Infrastructure.Data.Configurations;

public class IssueStatusHistoryConfiguration : IEntityTypeConfiguration<IssueStatusHistory>
{
    public void Configure(EntityTypeBuilder<IssueStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.FromStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.ToStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.Remarks)
            .HasMaxLength(1000);

        builder.HasIndex(h => h.ChangedAtUtc);

        builder.HasOne(h => h.Issue)
            .WithMany(i => i.StatusHistory)
            .HasForeignKey(h => h.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ChangedByUser)
            .WithMany(u => u.StatusChangesPerformed)
            .HasForeignKey(h => h.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
