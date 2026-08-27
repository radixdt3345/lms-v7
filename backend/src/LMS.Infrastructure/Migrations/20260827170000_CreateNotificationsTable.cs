using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace LMS.Infrastructure.Migrations;

public partial class CreateNotificationsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "notifications",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "varchar(200)", nullable: false),
                message = table.Column<string>(type: "text", nullable: false),
                type = table.Column<string>(type: "varchar(50)", nullable: false),
                is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                related_entity_type = table.Column<string>(type: "varchar(30)", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_notifications", x => x.id);
                table.ForeignKey("FK_notifications_users_user_id", x => x.user_id, "users", "id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("IX_notifications_user_id_is_read", "notifications", new[] { "user_id", "is_read" });
    }
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "notifications");
}
