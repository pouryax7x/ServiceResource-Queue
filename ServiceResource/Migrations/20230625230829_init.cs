using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceResource.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueueSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodName = table.Column<int>(type: "int", nullable: false),
                    MaxCallsPerInterval = table.Column<int>(type: "int", nullable: false),
                    Interval_Sec = table.Column<int>(type: "int", nullable: false),
                    CallBackMaxCallsPerInterval = table.Column<int>(type: "int", nullable: false),
                    CallBackInterval_Sec = table.Column<int>(type: "int", nullable: false),
                    CallBackAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueSetting", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueueSetting");
        }
    }
}
