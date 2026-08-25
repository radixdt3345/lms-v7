using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations;

public sealed class Rs256KeyConfiguration : IEntityTypeConfiguration<Rs256Key>
{
    public void Configure(EntityTypeBuilder<Rs256Key> builder)
    {
        builder.ToTable("rs256_keys");

        builder
            .HasKey(k => k.Id)
            .HasName("PK_rs256_keys");

        builder
            .Property(k => k.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder
            .Property(k => k.PublicKey)
            .HasColumnName("public_key")
            .HasColumnType("text")
            .IsRequired();

        builder
            .Property(k => k.PrivateKeyEncrypted)
            .HasColumnName("private_key_encrypted")
            .HasColumnType("text")
            .IsRequired();

        builder
            .Property(k => k.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasDefaultValue(true)
            .IsRequired();

        builder
            .Property(k => k.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder
            .Property(k => k.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAddOrUpdate()
            .IsRequired();
    }
}
