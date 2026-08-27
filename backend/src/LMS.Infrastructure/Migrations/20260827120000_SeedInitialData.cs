using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

public partial class SeedInitialData : Migration
{
    private const string PasswordHash = "$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lh0q";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO departments (id, name, code, is_active, created_at, updated_at)
            VALUES (gen_random_uuid(), 'Human Resources', 'HR', true, now(), now())
            ON CONFLICT (code) DO NOTHING;
        ");

        migrationBuilder.Sql(@"
            INSERT INTO users (id, name, email, password_hash, role, status, created_at, updated_at)
            VALUES (gen_random_uuid(), 'Super Admin', 'superadmin@company.com', '$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lh0q', 'SUPER_ADMIN', 'Active', now(), now())
            ON CONFLICT (email) DO NOTHING;
        ");

        migrationBuilder.Sql(@"
            INSERT INTO users (id, name, email, password_hash, role, status, created_at, updated_at)
            VALUES (gen_random_uuid(), 'HR Admin', 'hradmin@company.com', '$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lh0q', 'HR_ADMIN', 'Active', now(), now())
            ON CONFLICT (email) DO NOTHING;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM users WHERE email IN ('superadmin@company.com', 'hradmin@company.com');");
        migrationBuilder.Sql("DELETE FROM departments WHERE code = 'HR';");
    }
}
