using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CreateLeaveTypesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "leave_types",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                name = table.Column<string>(type: "varchar(100)", nullable: false),
                code = table.Column<string>(type: "varchar(10)", nullable: false),
                description = table.Column<string>(type: "varchar(500)", nullable: true),
                annual_days = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                requires_attachment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                requires_hr_approval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_leave_types", x => x.id);
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_leave_types_code",
            table: "leave_types",
            column: "code",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "ix_leave_types_name",
            table: "leave_types",
            column: "name",
            unique: true
        );

        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION update_updated_at_column()
            RETURNS TRIGGER AS $$
            BEGIN
                NEW.updated_at = NOW();
                RETURN NEW;
            END;
            $$ language 'plpgsql';
            """ +
            """
            CREATE TRIGGER tr_leave_types_updated_at
                BEFORE UPDATE ON leave_types
                FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
            """
        );

        // Seed 5 default leave types
        migrationBuilder.Sql(
            """
            INSERT INTO leave_types (id, name, code, description, annual_days, requires_attachment, requires_hr_approval, is_active, created_at, updated_at) VALUES
            ('00000000-0000-0000-0000-000000000001', 'Casual Leave', 'CL', 'Casual leave for personal matters', 12, false, false, true, NOW(), NOW()),
            ('00000000-0000-0000-0000-000000000002', 'Sick Leave', 'SL', 'Medical leave requiring documentation', 6, true, true, true, NOW(), NOW()),
            ('00000000-0000-0000-0000-000000000003', 'Earned Leave', 'EL', 'Accrued earned leave', 1, false, false, true, NOW(), NOW()),
            ('00000000-0000-0000-0000-000000000004', 'Comp-off', 'CO', 'Compensatory off for overtime worked', 0, false, false, true, NOW(), NOW()),
            ('00000000-0000-0000-0000-000000000005', 'Unpaid Leave', 'UL', 'Unpaid leave of absence', 0, false, false, true, NOW(), NOW())
            ON CONFLICT (id) DO NOTHING;
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "leave_types");
    }
}
