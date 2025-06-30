using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailEZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailIsHtml : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHtml",
                table: "Emails",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHtml",
                table: "Emails");
        }
    }
}
