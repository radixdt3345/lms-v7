using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddEmployeeProfileFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "phone",
            table: "users",
            type: "varchar",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "job_title",
            table: "users",
            type: "varchar",
            nullable: true);

        migrationBuilder.AddColumn<DateOnly>(
            name: "date_of_joining",
            table: "users",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "reporting_manager_id",
            table: "users",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "idx_users_reporting_manager_id",
            table: "users",
            column: "reporting_manager_id");

        migrationBuilder.AddForeignKey(
            name: "fk_users_reporting_manager",
            table: "users",
            column: "reporting_manager_id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_users_reporting_manager",
            table: "users");

        migrationBuilder.DropIndex(
            name: "idx_users_reporting_manager_id",
            table: "users");

        migrationBuilder.DropColumn(
            name: "reporting_manager_id",
            table: "users");

        migrationBuilder.DropColumn(
            name: "date_of_joining",
            table: "users");

        migrationBuilder.DropColumn(
            name: "job_title",
            table: "users");

        migrationBuilder.DropColumn(
            name: "phone",
            table: "users");
    }
}
