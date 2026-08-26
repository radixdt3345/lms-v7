using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_logs");
        b.HasKey(x => x.Id).HasName("pk_audit_logs");

        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasColumnType("varchar")
            .IsRequired();

        b.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("varchar")
            .IsRequired();

        b.Property(x => x.Action)
            .HasColumnName("action")
            .HasColumnType("varchar")
            .IsRequired();

        b.Property(x => x.ActorId)
            .HasColumnName("actor_id")
            .HasColumnType("uuid");

        b.Property(x => x.ActorEmail)
            .HasColumnName("actor_email")
            .HasColumnType("varchar");

        b.Property(x => x.Changes)
            .HasColumnName("changes")
            .HasColumnType("text");

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd();

        b.HasIndex(x => x.EntityType).HasDatabaseName("idx_audit_logs_entity_type");
        b.HasIndex(x => x.EntityId).HasDatabaseName("idx_audit_logs_entity_id");
        b.HasIndex(x => x.ActorId).HasDatabaseName("idx_audit_logs_actor_id");
    }
}
