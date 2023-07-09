using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceResource.Migrations
{
    /// <inheritdoc />
    public partial class changeScheduleToCron : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallBackInterval_Sec",
                table: "QueueSetting");

            migrationBuilder.DropColumn(
                name: "Interval_Sec",
                table: "QueueSetting");

            migrationBuilder.AddColumn<string>(
                name: "CallBackIntervalCronSchedule",
                table: "QueueSetting",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IntervalCronSchedule",
                table: "QueueSetting",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallBackIntervalCronSchedule",
                table: "QueueSetting");

            migrationBuilder.DropColumn(
                name: "IntervalCronSchedule",
                table: "QueueSetting");

            migrationBuilder.AddColumn<int>(
                name: "CallBackInterval_Sec",
                table: "QueueSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Interval_Sec",
                table: "QueueSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
