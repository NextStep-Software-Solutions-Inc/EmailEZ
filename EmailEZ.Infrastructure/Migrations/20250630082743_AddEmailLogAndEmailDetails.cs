using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailEZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailLogAndEmailDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ToAddresses",
                table: "Emails",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.AlterColumn<string>(
                name: "CcAddresses",
                table: "Emails",
                type: "text",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BccAddresses",
                table: "Emails",
                type: "text",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmailConfigurationId",
                table: "Emails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "Emails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpResponse",
                table: "Emails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromEmail",
                table: "EmailConfigurations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "EmailConfigurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Emails_EmailConfigurationId",
                table: "Emails",
                column: "EmailConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_EmailConfigurations_EmailConfigurationId",
                table: "Emails",
                column: "EmailConfigurationId",
                principalTable: "EmailConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emails_EmailConfigurations_EmailConfigurationId",
                table: "Emails");

            migrationBuilder.DropIndex(
                name: "IX_Emails_EmailConfigurationId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "EmailConfigurationId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "SmtpResponse",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "FromEmail",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "EmailConfigurations");

            migrationBuilder.AlterColumn<List<string>>(
                name: "ToAddresses",
                table: "Emails",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<List<string>>(
                name: "CcAddresses",
                table: "Emails",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<string>>(
                name: "BccAddresses",
                table: "Emails",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
