using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IMK_web.Migrations
{
    public partial class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "IMK_Functions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VSWR = table.Column<int>(type: "INTEGER", nullable: false),
                    FRU = table.Column<int>(type: "INTEGER", nullable: false),
                    CPRI = table.Column<int>(type: "INTEGER", nullable: false),
                    IPROUT = table.Column<int>(type: "INTEGER", nullable: false),
                    RetSerial = table.Column<int>(type: "INTEGER", nullable: false),
                    RSSIFDD = table.Column<int>(type: "INTEGER", nullable: false),
                    RSSIUMTS = table.Column<int>(type: "INTEGER", nullable: false),
                    RSSINR = table.Column<int>(type: "INTEGER", nullable: false),
                    IPInterfaces = table.Column<int>(type: "INTEGER", nullable: false),
                    RETAntenna = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IMK_Functions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImkVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateOfRelease = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AppVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    RPIVersion = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImkVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "Sites",
                columns: table => new
                {
                    SiteId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<string>(type: "TEXT", nullable: true),
                    longitude = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    OperatorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.SiteId);
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
                    VisitId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    SiteId = table.Column<string>(type: "TEXT", nullable: true),
                    IMK_FunctionsId = table.Column<int>(type: "INTEGER", nullable: true),
                    VistedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImkVersionId = table.Column<int>(type: "INTEGER", nullable: true)
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
                    LogId = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "json", nullable: true),
                    SiteVisitVisitId = table.Column<int>(type: "INTEGER", nullable: true)
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
                name: "IX_Logs_SiteVisitVisitId",
                table: "Logs",
                column: "SiteVisitVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Operators_CountryCode",
                table: "Operators",
                column: "CountryCode");

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
                name: "Countries");
        }
    }
}
