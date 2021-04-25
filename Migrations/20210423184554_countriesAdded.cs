using Microsoft.EntityFrameworkCore.Migrations;

namespace IMK_web.Migrations
{
    public partial class countriesAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspCompany_Countries_CountryCode",
                table: "AspCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_AspCompany_Users_ApsMentorUserId",
                table: "AspCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_AspCompany_AspCompanyAspId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_AspCompany_AspCompanyAspId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspCompany",
                table: "AspCompany");

            migrationBuilder.DropIndex(
                name: "IX_AspCompany_ApsMentorUserId",
                table: "AspCompany");

            migrationBuilder.DropColumn(
                name: "ApsMentorUserId",
                table: "AspCompany");

            migrationBuilder.RenameTable(
                name: "AspCompany",
                newName: "AspCompanies");

            migrationBuilder.RenameIndex(
                name: "IX_AspCompany_CountryCode",
                table: "AspCompanies",
                newName: "IX_AspCompanies_CountryCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspCompanies",
                table: "AspCompanies",
                column: "AspId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspCompanies_Countries_CountryCode",
                table: "AspCompanies",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_AspCompanies_AspCompanyAspId",
                table: "Sites",
                column: "AspCompanyAspId",
                principalTable: "AspCompanies",
                principalColumn: "AspId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AspCompanies_AspCompanyAspId",
                table: "Users",
                column: "AspCompanyAspId",
                principalTable: "AspCompanies",
                principalColumn: "AspId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspCompanies_Countries_CountryCode",
                table: "AspCompanies");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_AspCompanies_AspCompanyAspId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_AspCompanies_AspCompanyAspId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspCompanies",
                table: "AspCompanies");

            migrationBuilder.RenameTable(
                name: "AspCompanies",
                newName: "AspCompany");

            migrationBuilder.RenameIndex(
                name: "IX_AspCompanies_CountryCode",
                table: "AspCompany",
                newName: "IX_AspCompany_CountryCode");

            migrationBuilder.AddColumn<string>(
                name: "ApsMentorUserId",
                table: "AspCompany",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspCompany",
                table: "AspCompany",
                column: "AspId");

            migrationBuilder.CreateIndex(
                name: "IX_AspCompany_ApsMentorUserId",
                table: "AspCompany",
                column: "ApsMentorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspCompany_Countries_CountryCode",
                table: "AspCompany",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspCompany_Users_ApsMentorUserId",
                table: "AspCompany",
                column: "ApsMentorUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

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
    }
}
