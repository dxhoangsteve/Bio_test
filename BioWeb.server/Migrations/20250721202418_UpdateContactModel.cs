using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioWeb.server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContactModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookURL",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "GitHubURL",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "LinkedInURL",
                table: "Contacts");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "SiteConfigurations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FacebookURL",
                table: "SiteConfigurations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GitHubURL",
                table: "SiteConfigurations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkedInURL",
                table: "SiteConfigurations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "SiteConfigurations");

            migrationBuilder.DropColumn(
                name: "FacebookURL",
                table: "SiteConfigurations");

            migrationBuilder.DropColumn(
                name: "GitHubURL",
                table: "SiteConfigurations");

            migrationBuilder.DropColumn(
                name: "LinkedInURL",
                table: "SiteConfigurations");

            migrationBuilder.AddColumn<string>(
                name: "FacebookURL",
                table: "Contacts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GitHubURL",
                table: "Contacts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkedInURL",
                table: "Contacts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
