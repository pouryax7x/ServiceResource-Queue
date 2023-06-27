using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceResource.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxCallCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CallBackMaxCallCount",
                table: "QueueSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxCallCount",
                table: "QueueSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallBackMaxCallCount",
                table: "QueueSetting");

            migrationBuilder.DropColumn(
                name: "MaxCallCount",
                table: "QueueSetting");
        }
    }
}
