using BlogAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogAPI.Data.Mappings;

public class RefreshTokenMap : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(128);
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(128);

        // Computed in code, not stored
        builder.Ignore(t => t.IsRevoked);
        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsActive);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
