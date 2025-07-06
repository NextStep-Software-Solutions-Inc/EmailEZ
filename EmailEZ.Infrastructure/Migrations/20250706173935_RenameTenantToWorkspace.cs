using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailEZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTenantToWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailConfigurations_Tenants_TenantId",
                table: "EmailConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_Tenants_TenantId",
                table: "Emails");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Emails",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_Emails_TenantId",
                table: "Emails",
                newName: "IX_Emails_WorkspaceId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "EmailEvents",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailEvents_TenantId",
                table: "EmailEvents",
                newName: "IX_EmailEvents_WorkspaceId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "EmailConfigurations",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailConfigurations_TenantId",
                table: "EmailConfigurations",
                newName: "IX_EmailConfigurations_WorkspaceId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "EmailAttachments",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailAttachments_TenantId",
                table: "EmailAttachments",
                newName: "IX_EmailAttachments_WorkspaceId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "AuditLogs",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_TenantId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_WorkspaceId");

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ApiKeyHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApiKeyLastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_ApiKeyHash",
                table: "Workspaces",
                column: "ApiKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Domain",
                table: "Workspaces",
                column: "Domain",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Workspaces_WorkspaceId",
                table: "AuditLogs",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailConfigurations_Workspaces_WorkspaceId",
                table: "EmailConfigurations",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_Workspaces_WorkspaceId",
                table: "Emails",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Workspaces_WorkspaceId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailConfigurations_Workspaces_WorkspaceId",
                table: "EmailConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_Workspaces_WorkspaceId",
                table: "Emails");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "Emails",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Emails_WorkspaceId",
                table: "Emails",
                newName: "IX_Emails_TenantId");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "EmailEvents",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailEvents_WorkspaceId",
                table: "EmailEvents",
                newName: "IX_EmailEvents_TenantId");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "EmailConfigurations",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailConfigurations_WorkspaceId",
                table: "EmailConfigurations",
                newName: "IX_EmailConfigurations_TenantId");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "EmailAttachments",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailAttachments_WorkspaceId",
                table: "EmailAttachments",
                newName: "IX_EmailAttachments_TenantId");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "AuditLogs",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_WorkspaceId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_TenantId");

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiKeyHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ApiKeyLastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ApiKeyHash",
                table: "Tenants",
                column: "ApiKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Domain",
                table: "Tenants",
                column: "Domain",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId",
                table: "AuditLogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailConfigurations_Tenants_TenantId",
                table: "EmailConfigurations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_Tenants_TenantId",
                table: "Emails",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
