using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailEZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFastHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workspaces_ApiKeyHash",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "ApiKeyHash",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "ApiKeyLastUsedAt",
                table: "Workspaces");

            migrationBuilder.AddColumn<string>(
                name: "ApiKeyFastHash",
                table: "WorkspaceApiKeys",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKeyFastHash",
                table: "WorkspaceApiKeys");

            migrationBuilder.AddColumn<string>(
                name: "ApiKeyHash",
                table: "Workspaces",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApiKeyLastUsedAt",
                table: "Workspaces",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_ApiKeyHash",
                table: "Workspaces",
                column: "ApiKeyHash",
                unique: true);
        }
    }
}
