using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AndroidWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueAssignedWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedWorkerID",
                table: "Queues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Queues_AssignedWorkerID",
                table: "Queues",
                column: "AssignedWorkerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Queues_Users_AssignedWorkerID",
                table: "Queues",
                column: "AssignedWorkerID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queues_Users_AssignedWorkerID",
                table: "Queues");

            migrationBuilder.DropIndex(
                name: "IX_Queues_AssignedWorkerID",
                table: "Queues");

           
            migrationBuilder.DropColumn(
                name: "AssignedWorkerID",
                table: "Queues");
        }
    }
}
