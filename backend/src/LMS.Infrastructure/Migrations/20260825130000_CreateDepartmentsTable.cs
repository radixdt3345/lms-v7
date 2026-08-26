using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CreateDepartmentsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── departments ────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "departments",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                name = table.Column<string>(type: "varchar", nullable: false),
                code = table.Column<string>(type: "varchar(10)", nullable: false),
                overlap_limit = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                status = table.Column<string>(type: "varchar", nullable: false, defaultValue: "Active"),
                deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_departments", x => x.id);
                table.CheckConstraint("ck_departments_status", "status IN ('Active', 'Inactive')");
            }
        );

        migrationBuilder.CreateIndex(
            name: "idx_departments_name",
            table: "departments",
            column: "name",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "idx_departments_code",
            table: "departments",
            column: "code",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "idx_departments_status",
            table: "departments",
            column: "status"
        );

        migrationBuilder.Sql(
            """
            CREATE TRIGGER tr_departments_updated_at
                BEFORE UPDATE ON departments
                FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
            """
        );

        // ── FK: users.department_id → departments.id ───────────────────────────
        // users.department_id was added in the auth migration without a FK constraint
        // because departments did not yet exist. Now that departments exists, add the FK.
        migrationBuilder.CreateIndex(
            name: "idx_users_department_id",
            table: "users",
            column: "department_id"
        );

        migrationBuilder.AddForeignKey(
            name: "fk_users_departments",
            table: "users",
            column: "department_id",
            principalTable: "departments",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Remove FK before dropping the principal table.
        migrationBuilder.DropForeignKey(name: "fk_users_departments", table: "users");
        migrationBuilder.DropIndex(name: "idx_users_department_id", table: "users");
        migrationBuilder.DropTable(name: "departments");
    }
}
