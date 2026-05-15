using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SysScore.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedSecurityMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BootTime",
                table: "SystemDataRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "DiskFreeGb",
                table: "SystemDataRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "HighCpuProcessCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HighMemoryProcessCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LargestUnnecessaryFiles",
                table: "SystemDataRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ListeningPortCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NetworkConnectionCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "SwapUsage",
                table: "SystemDataRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "SystemUptimeSeconds",
                table: "SystemDataRecords",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "UnnecessaryFileCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UnnecessaryFileLocations",
                table: "SystemDataRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UnnecessaryFileSizeMb",
                table: "SystemDataRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BootTime",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "DiskFreeGb",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "HighCpuProcessCount",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "HighMemoryProcessCount",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "LargestUnnecessaryFiles",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "ListeningPortCount",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "NetworkConnectionCount",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "SwapUsage",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "SystemUptimeSeconds",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "UnnecessaryFileCount",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "UnnecessaryFileLocations",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "UnnecessaryFileSizeMb",
                table: "SystemDataRecords");
        }
    }
}
