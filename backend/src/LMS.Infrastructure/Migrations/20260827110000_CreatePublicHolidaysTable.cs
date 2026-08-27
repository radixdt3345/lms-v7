using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CreatePublicHolidaysTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "public_holidays",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                date = table.Column<DateOnly>(type: "date", nullable: false),
                name = table.Column<string>(type: "varchar(200)", nullable: false),
                year = table.Column<int>(type: "int", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_public_holidays", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_public_holidays_year",
            table: "public_holidays",
            column: "year");

        migrationBuilder.CreateIndex(
            name: "IX_public_holidays_date",
            table: "public_holidays",
            column: "date",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "public_holidays");
    }
}
