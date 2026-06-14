using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SysScore.Migrations
{
    /// <inheritdoc />
    public partial class AddThreatEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThreatEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemDataId = table.Column<int>(type: "int", nullable: false),
                    ThreatType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ThreatLevel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ThreatScore = table.Column<int>(type: "int", nullable: false),
                    Evidence = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    RecommendedActions = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreatEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThreatEvents_SystemDataRecords_SystemDataId",
                        column: x => x.SystemDataId,
                        principalTable: "SystemDataRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThreatEvents_SystemDataId",
                table: "ThreatEvents",
                column: "SystemDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThreatEvents");
        }
    }
}
