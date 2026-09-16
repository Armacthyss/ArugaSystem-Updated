using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AndroidWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyQueueUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Queues_QueueDate_QueueNumber",
                table: "Queues",
                columns: new[] { "QueueDate", "QueueNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Queues_QueueDate_QueueNumber",
                table: "Queues");
        }
    }
}