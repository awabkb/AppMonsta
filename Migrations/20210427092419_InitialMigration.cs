using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IMK_web.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "IMK_Functions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VSWR = table.Column<int>(type: "int", nullable: false),
                    FRU = table.Column<int>(type: "int", nullable: false),
                    CPRI = table.Column<int>(type: "int", nullable: false),
                    RSSILTE = table.Column<int>(type: "int", nullable: false),
                    RSSIUMTS = table.Column<int>(type: "int", nullable: false),
                    RSSINR = table.Column<int>(type: "int", nullable: false),
                    IPROUT = table.Column<int>(type: "int", nullable: false),
                    IPInterfaces = table.Column<int>(type: "int", nullable: false),
                    RETAntenna = table.Column<int>(type: "int", nullable: false),
                    RetSerial = table.Column<int>(type: "int", nullable: false),
                    Alarms = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IMK_Functions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImkVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DateOfRelease = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AppVersion = table.Column<double>(type: "double", nullable: false),
                    RPIVersion = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImkVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspCompanies",
                columns: table => new
                {
                    AspId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    CountryCode = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspCompanies", x => x.AspId);
                    table.ForeignKey(
                        name: "FK_AspCompanies_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    CountryCode = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operators_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Email = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Phone = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    AspCompanyAspId = table.Column<int>(type: "int", nullable: true),
                    IsAdmin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_AspCompanies_AspCompanyAspId",
                        column: x => x.AspCompanyAspId,
                        principalTable: "AspCompanies",
                        principalColumn: "AspId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    SiteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Latitude = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    longitude = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Country = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    OperatorId = table.Column<int>(type: "int", nullable: true),
                    AspCompanyAspId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.SiteId);
                    table.ForeignKey(
                        name: "FK_Sites_AspCompanies_AspCompanyAspId",
                        column: x => x.AspCompanyAspId,
                        principalTable: "AspCompanies",
                        principalColumn: "AspId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sites_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SiteVisits",
                columns: table => new
                {
                    VisitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true),
                    IMK_FunctionsId = table.Column<int>(type: "int", nullable: true),
                    VistedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ImkVersionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteVisits", x => x.VisitId);
                    table.ForeignKey(
                        name: "FK_SiteVisits_IMK_Functions_IMK_FunctionsId",
                        column: x => x.IMK_FunctionsId,
                        principalTable: "IMK_Functions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteVisits_ImkVersions_ImkVersionId",
                        column: x => x.ImkVersionId,
                        principalTable: "ImkVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteVisits_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "SiteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteVisits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Command = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Result = table.Column<string>(type: "json", nullable: true),
                    SiteVisitVisitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_Logs_SiteVisits_SiteVisitVisitId",
                        column: x => x.SiteVisitVisitId,
                        principalTable: "SiteVisits",
                        principalColumn: "VisitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspCompanies_CountryCode",
                table: "AspCompanies",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_SiteVisitVisitId",
                table: "Logs",
                column: "SiteVisitVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Operators_CountryCode",
                table: "Operators",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_AspCompanyAspId",
                table: "Sites",
                column: "AspCompanyAspId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_OperatorId",
                table: "Sites",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteVisits_IMK_FunctionsId",
                table: "SiteVisits",
                column: "IMK_FunctionsId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteVisits_ImkVersionId",
                table: "SiteVisits",
                column: "ImkVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteVisits_SiteId",
                table: "SiteVisits",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteVisits_UserId",
                table: "SiteVisits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AspCompanyAspId",
                table: "Users",
                column: "AspCompanyAspId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "SiteVisits");

            migrationBuilder.DropTable(
                name: "IMK_Functions");

            migrationBuilder.DropTable(
                name: "ImkVersions");

            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "AspCompanies");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
