using Microsoft.EntityFrameworkCore.Migrations;

namespace IMK_web.Migrations
{
    public partial class phoneAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Action",
                table: "Logs",
                newName: "Command");

            migrationBuilder.RenameColumn(
                name: "RSSIFDD",
                table: "IMK_Functions",
                newName: "RSSILTE");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Logs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Logs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "Logs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "RPIVersion",
                table: "ImkVersions",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "AppVersion",
                table: "ImkVersions",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "Logs");

            migrationBuilder.RenameColumn(
                name: "Command",
                table: "Logs",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "RSSILTE",
                table: "IMK_Functions",
                newName: "RSSIFDD");

            migrationBuilder.AlterColumn<int>(
                name: "RPIVersion",
                table: "ImkVersions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "AppVersion",
                table: "ImkVersions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");
        }
    }
}
