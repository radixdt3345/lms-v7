using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CreateAuditLogsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "audit_logs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                entity_type = table.Column<string>(type: "varchar", nullable: false),
                entity_id = table.Column<string>(type: "varchar", nullable: false),
                action = table.Column<string>(type: "varchar", nullable: false),
                actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                actor_email = table.Column<string>(type: "varchar", nullable: true),
                changes = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_audit_logs", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "idx_audit_logs_actor_id",
            table: "audit_logs",
            column: "actor_id");

        migrationBuilder.CreateIndex(
            name: "idx_audit_logs_entity_id",
            table: "audit_logs",
            column: "entity_id");

        migrationBuilder.CreateIndex(
            name: "idx_audit_logs_entity_type",
            table: "audit_logs",
            column: "entity_type");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "audit_logs");
    }
}
