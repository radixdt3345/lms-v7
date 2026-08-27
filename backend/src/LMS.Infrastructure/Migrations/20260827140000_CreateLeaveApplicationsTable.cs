using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

public partial class CreateLeaveApplicationsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "leave_applications",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                leave_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                start_date = table.Column<DateOnly>(type: "date", nullable: false),
                end_date = table.Column<DateOnly>(type: "date", nullable: false),
                total_days = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
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
                table.PrimaryKey("PK_leave_applications", x => x.id);
                table.ForeignKey("FK_leave_applications_users_employee_id", x => x.employee_id, "users", "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_leave_applications_leave_types_leave_type_id", x => x.leave_type_id, "leave_types", "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_leave_applications_users_approved_by_id", x => x.approved_by_id, "users", "id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_leave_applications_employee_id", "leave_applications", "employee_id");
        migrationBuilder.CreateIndex("IX_leave_applications_status", "leave_applications", "status");
        migrationBuilder.CreateIndex("IX_leave_applications_leave_type_id", "leave_applications", "leave_type_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "leave_applications");
    }
}
