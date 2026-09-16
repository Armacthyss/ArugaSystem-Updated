using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AndroidWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueAssignmentConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "Queues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentRespondedAt",
                table: "Queues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentStatus",
                table: "Queues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "Queues");

            migrationBuilder.DropColumn(
                name: "AssignmentRespondedAt",
                table: "Queues");

            migrationBuilder.DropColumn(
                name: "AssignmentStatus",
                table: "Queues");
        }
    }
}
