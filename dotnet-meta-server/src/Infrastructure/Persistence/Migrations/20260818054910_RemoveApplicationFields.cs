using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApplicationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pipeline_templates_applications_ApplicationId",
                table: "pipeline_templates");

            migrationBuilder.DropIndex(
                name: "IX_pipeline_templates_ApplicationId",
                table: "pipeline_templates");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "pipeline_templates");

            migrationBuilder.DropColumn(
                name: "deploy_key",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "dev_branch",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "main_branch",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "pre_branch",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "ranchers",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "stage_branch",
                table: "applications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationId",
                table: "pipeline_templates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deploy_key",
                table: "applications",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dev_branch",
                table: "applications",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "dev");

            migrationBuilder.AddColumn<string>(
                name: "main_branch",
                table: "applications",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "main");

            migrationBuilder.AddColumn<string>(
                name: "pre_branch",
                table: "applications",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "pre");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "ranchers",
                table: "applications",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stage_branch",
                table: "applications",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "stage");

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_templates_ApplicationId",
                table: "pipeline_templates",
                column: "ApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_pipeline_templates_applications_ApplicationId",
                table: "pipeline_templates",
                column: "ApplicationId",
                principalTable: "applications",
                principalColumn: "id");
        }
    }
}
