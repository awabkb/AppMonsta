using Microsoft.EntityFrameworkCore.Migrations;

namespace IMK_web.Migrations
{
    public partial class finalmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Alarms",
                table: "IMK_Functions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alarms",
                table: "IMK_Functions");
        }
    }
}
