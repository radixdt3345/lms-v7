using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder
            .HasKey(d => d.Id)
            .HasName("PK_departments");

        builder
            .Property(d => d.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder
            .Property(d => d.Name)
            .HasColumnName("name")
            .HasColumnType("varchar")
            .IsRequired();

        builder
            .HasIndex(d => d.Name)
            .IsUnique()
            .HasDatabaseName("idx_departments_name");

        builder
            .Property(d => d.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(10)")
            .IsRequired();

        builder
            .HasIndex(d => d.Code)
            .IsUnique()
            .HasDatabaseName("idx_departments_code");

        builder
            .Property(d => d.OverlapLimit)
            .HasColumnName("overlap_limit")
            .HasColumnType("integer")
            .HasDefaultValue(2)
            .IsRequired();

        builder
            .Property(d => d.Status)
            .HasColumnName("status")
            .HasColumnType("varchar")
            .HasDefaultValue("Active")
            .IsRequired();

        builder.HasCheckConstraint(
            "ck_departments_status",
            "status IN ('Active', 'Inactive')"
        );

        builder
            .HasIndex(d => d.Status)
            .HasDatabaseName("idx_departments_status");

        builder
            .Property(d => d.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz");

        builder
            .Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder
            .Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAddOrUpdate()
            .IsRequired();

        // FK: users.department_id → departments.id (SetNull on deactivation)
        builder
            .HasMany(d => d.Users)
            .WithOne(u => u.Department)
            .HasForeignKey(u => u.DepartmentId)
            .HasConstraintName("fk_users_departments")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
