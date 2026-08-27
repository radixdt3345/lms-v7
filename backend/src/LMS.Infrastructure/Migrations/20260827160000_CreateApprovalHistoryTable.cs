using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace LMS.Infrastructure.Migrations;

public partial class CreateApprovalHistoryTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "approval_history",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                entity_type = table.Column<string>(type: "varchar(30)", nullable: false),
                entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "varchar(20)", nullable: false),
                comments = table.Column<string>(type: "text", nullable: true),
                acted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_approval_history", x => x.id);
                table.ForeignKey("FK_approval_history_users_actor_id", x => x.actor_id, "users", "id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_approval_history_entity_type_entity_id", "approval_history", new[] { "entity_type", "entity_id" });
        migrationBuilder.CreateIndex("IX_approval_history_actor_id", "approval_history", "actor_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "approval_history");
    }
}
