using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CreateAuthTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── Shared trigger function ────────────────────────────────────────────
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION update_updated_at_column()
            RETURNS TRIGGER AS $$
            BEGIN
                NEW.updated_at = NOW();
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;
            """
        );

        // ── users ──────────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                name = table.Column<string>(type: "varchar", nullable: false),
                email = table.Column<string>(type: "varchar", nullable: false),
                password_hash = table.Column<string>(type: "varchar", nullable: true),
                role = table.Column<string>(type: "varchar", nullable: false, defaultValue: "EMPLOYEE"),
                status = table.Column<string>(type: "varchar", nullable: false, defaultValue: "Active"),
                department_id = table.Column<Guid>(type: "uuid", nullable: true),
                failed_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                locked_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users", x => x.id);
                table.CheckConstraint("ck_users_role", "role IN ('EMPLOYEE', 'MANAGER', 'HR_ADMIN', 'SUPER_ADMIN')");
                table.CheckConstraint("ck_users_status", "status IN ('Active', 'Inactive', 'Locked')");
            }
        );

        migrationBuilder.CreateIndex(name: "idx_users_email", table: "users", column: "email", unique: true);
        migrationBuilder.CreateIndex(name: "idx_users_status", table: "users", column: "status");

        migrationBuilder.Sql(
            """
            CREATE TRIGGER tr_users_updated_at
                BEFORE UPDATE ON users
                FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
            """
        );

        // ── refresh_tokens ─────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "varchar", nullable: false),
                expires_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                revoked_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                ip_address = table.Column<string>(type: "varchar", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_refresh_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_refresh_tokens_users",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(name: "idx_refresh_tokens_user_id", table: "refresh_tokens", column: "user_id");

        migrationBuilder.Sql(
            """
            CREATE TRIGGER tr_refresh_tokens_updated_at
                BEFORE UPDATE ON refresh_tokens
                FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
            """
        );

        // ── rs256_keys ─────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "rs256_keys",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                public_key = table.Column<string>(type: "text", nullable: false),
                private_key_encrypted = table.Column<string>(type: "text", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_rs256_keys", x => x.id);
            }
        );

        migrationBuilder.Sql(
            """
            CREATE TRIGGER tr_rs256_keys_updated_at
                BEFORE UPDATE ON rs256_keys
                FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop tables in reverse dependency order.
        // Triggers on each table are dropped automatically when the table is dropped.
        migrationBuilder.DropTable(name: "refresh_tokens");
        migrationBuilder.DropTable(name: "rs256_keys");
        migrationBuilder.DropTable(name: "users");

        // Drop the shared trigger function after all tables that used it are gone.
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_updated_at_column();");
    }
}
