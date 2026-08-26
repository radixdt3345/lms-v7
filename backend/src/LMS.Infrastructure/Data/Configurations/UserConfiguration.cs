using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder
            .HasKey(u => u.Id)
            .HasName("PK_users");

        builder
            .Property(u => u.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder
            .Property(u => u.Name)
            .HasColumnName("name")
            .HasColumnType("varchar")
            .IsRequired();

        builder
            .Property(u => u.Email)
            .HasColumnName("email")
            .HasColumnType("varchar")
            .IsRequired();

        builder
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("idx_users_email");

        builder
            .Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar");

        builder
            .Property(u => u.Role)
            .HasColumnName("role")
            .HasColumnType("varchar")
            .HasDefaultValue("EMPLOYEE")
            .IsRequired();

        builder.HasCheckConstraint(
            "ck_users_role",
            "role IN ('EMPLOYEE', 'MANAGER', 'HR_ADMIN', 'SUPER_ADMIN')"
        );

        builder
            .Property(u => u.Status)
            .HasColumnName("status")
            .HasColumnType("varchar")
            .HasDefaultValue("Active")
            .IsRequired();

        builder.HasCheckConstraint(
            "ck_users_status",
            "status IN ('Active', 'Inactive', 'Locked')"
        );

        // Separate index supports FR-8: listing locked accounts
        builder
            .HasIndex(u => u.Status)
            .HasDatabaseName("idx_users_status");

        builder
            .Property(u => u.DepartmentId)
            .HasColumnName("department_id")
            .HasColumnType("uuid");

        builder
            .Property(u => u.FailedAttempts)
            .HasColumnName("failed_attempts")
            .HasColumnType("integer")
            .HasDefaultValue(0)
            .IsRequired();

        builder
            .Property(u => u.LockedAt)
            .HasColumnName("locked_at")
            .HasColumnType("timestamptz");

        builder
            .Property(u => u.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz");

        builder
            .Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder
            .Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAddOrUpdate()
            .IsRequired();

        builder
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .HasConstraintName("fk_refresh_tokens_users")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
