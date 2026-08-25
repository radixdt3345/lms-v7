using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder
            .HasKey(rt => rt.Id)
            .HasName("PK_refresh_tokens");

        builder
            .Property(rt => rt.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder
            .Property(rt => rt.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder
            .Property(rt => rt.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar")
            .IsRequired();

        builder
            .Property(rt => rt.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder
            .Property(rt => rt.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz");

        builder
            .Property(rt => rt.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("varchar")
            .IsRequired();

        builder
            .Property(rt => rt.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder
            .Property(rt => rt.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAddOrUpdate()
            .IsRequired();

        builder
            .HasIndex(rt => rt.UserId)
            .HasDatabaseName("idx_refresh_tokens_user_id");

        // FK to users is configured from UserConfiguration via HasMany/WithOne
    }
}
