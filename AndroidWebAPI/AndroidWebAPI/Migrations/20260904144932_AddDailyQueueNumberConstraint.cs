using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AndroidWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyQueueNumberConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Queues_QueueNumber_Range",
                table: "Queues",
                sql: "[QueueNumber] >= 1 AND [QueueNumber] <= 999");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Queues_QueueNumber_Range",
                table: "Queues");
        }
    }
}
