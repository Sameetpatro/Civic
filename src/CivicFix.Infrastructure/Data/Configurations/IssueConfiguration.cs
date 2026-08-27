using CivicFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFix.Infrastructure.Data.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.ReferenceNumber)
            .IsUnique();

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(i => i.Description)
            .IsRequired();

        builder.Property(i => i.Address)
            .HasMaxLength(300);

        builder.Property(i => i.WardSector)
            .IsRequired()
            .HasMaxLength(100);

        // Geospatial coordinates
        builder.Property(i => i.Latitude)
            .HasPrecision(9, 6);

        builder.Property(i => i.Longitude)
            .HasPrecision(9, 6);

        builder.HasIndex(i => new { i.Latitude, i.Longitude });
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.ReportedAtUtc);
        builder.HasIndex(i => i.WardSector);

        // Enums
        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.Priority)
            .IsRequired()
            .HasConversion<int>();

        // Relationships
        builder.HasOne(i => i.Category)
            .WithMany(c => c.Issues)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Department)
            .WithMany(d => d.Issues)
            .HasForeignKey(i => i.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.ReportedByUser)
            .WithMany(u => u.ReportedIssues)
            .HasForeignKey(i => i.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.AssignedWorker)
            .WithMany(u => u.AssignedIssues)
            .HasForeignKey(i => i.AssignedWorkerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.MasterIssue)
            .WithMany(m => m.DuplicateIssues)
            .HasForeignKey(i => i.MasterIssueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
