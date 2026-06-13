using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SysScore.Migrations
{
    /// <inheritdoc />
    public partial class AddThreatDetectionMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstablishedConnectionCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "InboundPacketRate",
                table: "SystemDataRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "NetworkConnectionDelta",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "OutboundPacketRate",
                table: "SystemDataRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedActions",
                table: "SystemDataRecords",
                type: "nvarchar(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SynSentConnectionCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThreatDetectedAt",
                table: "SystemDataRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThreatEvidence",
                table: "SystemDataRecords",
                type: "nvarchar(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThreatLevel",
                table: "SystemDataRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThreatScore",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ThreatType",
                table: "SystemDataRecords",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeWaitConnectionCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UniqueRemoteAddressCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UniqueRemotePortCount",
                table: "SystemDataRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstablishedConnectionCount",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "InboundPacketRate",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "NetworkConnectionDelta",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "OutboundPacketRate",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "RecommendedActions",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "SynSentConnectionCount",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "ThreatDetectedAt",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "ThreatEvidence",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "ThreatLevel",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "ThreatScore",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "ThreatType",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "TimeWaitConnectionCount",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "UniqueRemoteAddressCount",
                table: "SystemDataRecords");

            migrationBuilder.DropColumn(
                name: "UniqueRemotePortCount",
                table: "SystemDataRecords");
        }
    }
}
