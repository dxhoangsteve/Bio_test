using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioWeb.server.Migrations
{
    /// <inheritdoc />
    public partial class AddReadCountToContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReadCount",
                table: "Contacts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadCount",
                table: "Contacts");
        }
    }
}
