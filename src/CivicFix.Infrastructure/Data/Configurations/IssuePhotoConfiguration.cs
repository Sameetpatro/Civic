using CivicFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFix.Infrastructure.Data.Configurations;

public class IssuePhotoConfiguration : IEntityTypeConfiguration<IssuePhoto>
{
    public void Configure(EntityTypeBuilder<IssuePhoto> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.PhotoUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Caption)
            .HasMaxLength(250);

        builder.Property(p => p.PhotoType)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne(p => p.Issue)
            .WithMany(i => i.Photos)
            .HasForeignKey(p => p.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.UploadedByUser)
            .WithMany()
            .HasForeignKey(p => p.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
