using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioWeb.server.Migrations
{
    /// <inheritdoc />
    public partial class RefactorArticleAndAddSiteModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_AdminUsers_AuthorID",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_AuthorID",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "AuthorID",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ThumbnailURL",
                table: "Articles");

            migrationBuilder.CreateTable(
                name: "AboutMes",
                columns: table => new
                {
                    AboutMeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AvatarURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BioSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutMes", x => x.AboutMeID);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    ContactID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GitHubURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LinkedInURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FacebookURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.ContactID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutMes");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.AddColumn<int>(
                name: "AuthorID",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailURL",
                table: "Articles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_AuthorID",
                table: "Articles",
                column: "AuthorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_AdminUsers_AuthorID",
                table: "Articles",
                column: "AuthorID",
                principalTable: "AdminUsers",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
