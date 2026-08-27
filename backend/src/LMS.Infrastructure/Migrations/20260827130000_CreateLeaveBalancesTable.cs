using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

public partial class CreateLeaveBalancesTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "leave_balances",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                leave_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                year = table.Column<int>(type: "integer", nullable: false),
                total_days = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                used_days = table.Column<decimal>(type: "decimal(5,1)", nullable: false, defaultValue: 0m),
                pending_days = table.Column<decimal>(type: "decimal(5,1)", nullable: false, defaultValue: 0m),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_leave_balances", x => x.id);
                table.ForeignKey(
                    name: "FK_leave_balances_leave_types_leave_type_id",
                    column: x => x.leave_type_id,
                    principalTable: "leave_types",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_leave_balances_users_employee_id",
                    column: x => x.employee_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_leave_balances_employee_id_leave_type_id_year",
            table: "leave_balances",
            columns: new[] { "employee_id", "leave_type_id", "year" },
            unique: true);

        migrationBuilder.CreateTable(
            name: "comp_off_credits",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                earned_date = table.Column<DateOnly>(type: "date", nullable: false),
                expiry_date = table.Column<DateOnly>(type: "date", nullable: false),
                credit_days = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                status = table.Column<string>(type: "varchar(10)", nullable: false, defaultValue: "Active"),
                comp_off_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_comp_off_credits", x => x.id);
                table.ForeignKey(
                    name: "FK_comp_off_credits_users_employee_id",
                    column: x => x.employee_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_comp_off_credits_employee_id",
            table: "comp_off_credits",
            column: "employee_id");

        migrationBuilder.CreateIndex(
            name: "IX_comp_off_credits_status",
            table: "comp_off_credits",
            column: "status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "comp_off_credits");
        migrationBuilder.DropTable(name: "leave_balances");
    }
}
