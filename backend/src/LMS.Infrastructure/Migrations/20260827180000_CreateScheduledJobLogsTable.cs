using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

public partial class CreateScheduledJobLogsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "scheduled_job_logs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                job_name = table.Column<string>(type: "varchar(100)", nullable: false),
                status = table.Column<string>(type: "varchar(20)", nullable: false),
                details = table.Column<string>(type: "text", nullable: true),
                started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                triggered_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_scheduled_job_logs", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_scheduled_job_logs_job_name",
            table: "scheduled_job_logs",
            column: "job_name");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "scheduled_job_logs");
    }
}
