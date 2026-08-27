using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

public partial class CreateCompOffRequestsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "comp_off_requests",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                worked_date = table.Column<DateOnly>(type: "date", nullable: false),
                credit_days = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                reason = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Pending"),
                approved_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                rejection_reason = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_comp_off_requests", x => x.id);
                table.ForeignKey("FK_comp_off_requests_users_employee_id", x => x.employee_id, "users", "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_comp_off_requests_users_approved_by_id", x => x.approved_by_id, "users", "id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_comp_off_requests_employee_id", "comp_off_requests", "employee_id");
        migrationBuilder.CreateIndex("IX_comp_off_requests_status", "comp_off_requests", "status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "comp_off_requests");
    }
}
