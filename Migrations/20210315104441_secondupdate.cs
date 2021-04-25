using Microsoft.EntityFrameworkCore.Migrations;

namespace IMK_web.Migrations
{
    public partial class secondupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AspCompanyAspId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AspCompanyAspId",
                table: "Sites",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AspCompany",
                columns: table => new
                {
                    AspId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ApsMentorUserId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspCompany", x => x.AspId);
                    table.ForeignKey(
                        name: "FK_AspCompany_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspCompany_Users_ApsMentorUserId",
                        column: x => x.ApsMentorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_AspCompanyAspId",
                table: "Users",
                column: "AspCompanyAspId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_AspCompanyAspId",
                table: "Sites",
                column: "AspCompanyAspId");

            migrationBuilder.CreateIndex(
                name: "IX_AspCompany_ApsMentorUserId",
                table: "AspCompany",
                column: "ApsMentorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspCompany_CountryCode",
                table: "AspCompany",
                column: "CountryCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_AspCompany_AspCompanyAspId",
                table: "Sites",
                column: "AspCompanyAspId",
                principalTable: "AspCompany",
                principalColumn: "AspId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AspCompany_AspCompanyAspId",
                table: "Users",
                column: "AspCompanyAspId",
                principalTable: "AspCompany",
                principalColumn: "AspId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_AspCompany_AspCompanyAspId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_AspCompany_AspCompanyAspId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AspCompany");

            migrationBuilder.DropIndex(
                name: "IX_Users_AspCompanyAspId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Sites_AspCompanyAspId",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "AspCompanyAspId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AspCompanyAspId",
                table: "Sites");
        }
    }
}
