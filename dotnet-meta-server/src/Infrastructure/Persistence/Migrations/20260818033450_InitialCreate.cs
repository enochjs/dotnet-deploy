using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_monitors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    env = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    version = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    source_uuid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    tenant_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    browser = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    message = table.Column<string>(type: "text", nullable: true),
                    stack = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    remark = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    resolved_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_monitors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    app_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    project_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "fe"),
                    deploy_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    git_id = table.Column<int>(type: "integer", nullable: false),
                    registry_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "fe"),
                    git_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    git_repo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    main_branch = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "main"),
                    pre_branch = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "pre"),
                    stage_branch = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "stage"),
                    dev_branch = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "dev"),
                    git_namespace_id = table.Column<int>(type: "integer", nullable: false),
                    trigger_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    owner_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    remark = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ranchers = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    updated_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "integration_releases",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    branch = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    remark = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_releases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "requirements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    document_url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: true),
                    remark = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    online_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    submitted_test_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    updated_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requirements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ding_talk_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    manager_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    manager_ding_talk_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    real_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    mobile = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sub_applications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_application_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    app_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    platform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    deploy_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    git_id = table.Column<int>(type: "integer", nullable: false),
                    registry_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "fe"),
                    git_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    git_repo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    main_branch = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "main"),
                    pre_branch = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "pre"),
                    stage_branch = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "stage"),
                    dev_branch = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "dev"),
                    prod_site_address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    pre_site_address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    stage_site_address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    dev_site_address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    git_namespace_id = table.Column<int>(type: "integer", nullable: false),
                    trigger_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    remark = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    public_path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    upload_to_oss = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    app_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "saas"),
                    variables = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    updated_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sub_applications", x => x.id);
                    table.ForeignKey(
                        name: "FK_sub_applications_applications_parent_application_id",
                        column: x => x.parent_application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "requirement_developers",
                columns: table => new
                {
                    requirement_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requirement_developers", x => new { x.requirement_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_requirement_developers_requirements_requirement_id",
                        column: x => x.requirement_id,
                        principalTable: "requirements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_requirement_developers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "requirement_followers",
                columns: table => new
                {
                    requirement_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requirement_followers", x => new { x.requirement_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_requirement_followers_requirements_requirement_id",
                        column: x => x.requirement_id,
                        principalTable: "requirements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_requirement_followers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iterations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    sub_application_id = table.Column<int>(type: "integer", nullable: true),
                    integration_release_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    application_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    sub_application_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    branch = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    original_commit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    remark = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    updated_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_iterations", x => x.id);
                    table.ForeignKey(
                        name: "FK_iterations_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_iterations_integration_releases_integration_release_id",
                        column: x => x.integration_release_id,
                        principalTable: "integration_releases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_iterations_sub_applications_sub_application_id",
                        column: x => x.sub_application_id,
                        principalTable: "sub_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pipeline_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    template_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    updated_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApplicationId = table.Column<int>(type: "integer", nullable: true),
                    SubApplicationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pipeline_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_pipeline_templates_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_pipeline_templates_sub_applications_SubApplicationId",
                        column: x => x.SubApplicationId,
                        principalTable: "sub_applications",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "integration_release_apps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    integration_release_id = table.Column<int>(type: "integer", nullable: false),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    sub_application_id = table.Column<int>(type: "integer", nullable: false),
                    app_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    application_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    sub_application_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    iteration_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_release_apps", x => x.id);
                    table.ForeignKey(
                        name: "FK_integration_release_apps_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_integration_release_apps_integration_releases_integration_r~",
                        column: x => x.integration_release_id,
                        principalTable: "integration_releases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_integration_release_apps_iterations_iteration_id",
                        column: x => x.iteration_id,
                        principalTable: "iterations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_integration_release_apps_sub_applications_sub_application_id",
                        column: x => x.sub_application_id,
                        principalTable: "sub_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "iteration_requirements",
                columns: table => new
                {
                    iteration_id = table.Column<int>(type: "integer", nullable: false),
                    requirement_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_iteration_requirements", x => new { x.iteration_id, x.requirement_id });
                    table.ForeignKey(
                        name: "FK_iteration_requirements_iterations_iteration_id",
                        column: x => x.iteration_id,
                        principalTable: "iterations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_iteration_requirements_requirements_requirement_id",
                        column: x => x.requirement_id,
                        principalTable: "requirements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pipeline_template_stages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pipeline_template_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    seq = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pipeline_template_stages", x => x.id);
                    table.ForeignKey(
                        name: "FK_pipeline_template_stages_pipeline_templates_pipeline_templa~",
                        column: x => x.pipeline_template_id,
                        principalTable: "pipeline_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pipelines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    iteration_id = table.Column<int>(type: "integer", nullable: false),
                    repo_id = table.Column<int>(type: "integer", nullable: false),
                    registry_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "fe"),
                    created_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    stage_seq = table.Column<int>(type: "integer", nullable: false, defaultValue: -1),
                    pipeline_template_id = table.Column<int>(type: "integer", nullable: false),
                    branch = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    content = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    swim_lane = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    force_update = table.Column<int>(type: "integer", nullable: true),
                    extra = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pipelines", x => x.id);
                    table.ForeignKey(
                        name: "FK_pipelines_pipeline_templates_pipeline_template_id",
                        column: x => x.pipeline_template_id,
                        principalTable: "pipeline_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pipeline_template_jobs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pipeline_template_stage_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    job_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    stage_seq = table.Column<int>(type: "integer", nullable: false),
                    extra = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pipeline_template_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_pipeline_template_jobs_pipeline_template_stages_pipeline_te~",
                        column: x => x.pipeline_template_stage_id,
                        principalTable: "pipeline_template_stages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deploys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pipeline_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    iteration_id = table.Column<int>(type: "integer", nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by_user_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    env = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    version = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    use_vpn = table.Column<bool>(type: "boolean", nullable: true),
                    deploy_type = table.Column<int>(type: "integer", nullable: true),
                    swim_lane = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    integration_release_version = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deploys", x => x.id);
                    table.ForeignKey(
                        name: "FK_deploys_pipelines_pipeline_id",
                        column: x => x.pipeline_id,
                        principalTable: "pipelines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "pipeline_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pipeline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stage_seq = table.Column<int>(type: "integer", nullable: false),
                    job_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    unit_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    extra = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pipeline_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_pipeline_jobs_pipelines_pipeline_id",
                        column: x => x.pipeline_id,
                        principalTable: "pipelines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_monitors_app_key_env",
                table: "app_monitors",
                columns: new[] { "app_key", "env" });

            migrationBuilder.CreateIndex(
                name: "ix_app_monitors_status",
                table: "app_monitors",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_applications_app_key",
                table: "applications",
                column: "app_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_deploys_app_key",
                table: "deploys",
                column: "app_key");

            migrationBuilder.CreateIndex(
                name: "ix_deploys_pipeline_id",
                table: "deploys",
                column: "pipeline_id");

            migrationBuilder.CreateIndex(
                name: "IX_integration_release_apps_application_id",
                table: "integration_release_apps",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_integration_release_apps_integration_release_id",
                table: "integration_release_apps",
                column: "integration_release_id");

            migrationBuilder.CreateIndex(
                name: "IX_integration_release_apps_iteration_id",
                table: "integration_release_apps",
                column: "iteration_id");

            migrationBuilder.CreateIndex(
                name: "IX_integration_release_apps_sub_application_id",
                table: "integration_release_apps",
                column: "sub_application_id");

            migrationBuilder.CreateIndex(
                name: "ix_integration_releases_version",
                table: "integration_releases",
                column: "version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_iteration_requirements_requirement_id",
                table: "iteration_requirements",
                column: "requirement_id");

            migrationBuilder.CreateIndex(
                name: "IX_iterations_application_id",
                table: "iterations",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_iterations_integration_release_id",
                table: "iterations",
                column: "integration_release_id");

            migrationBuilder.CreateIndex(
                name: "IX_iterations_sub_application_id",
                table: "iterations",
                column: "sub_application_id");

            migrationBuilder.CreateIndex(
                name: "ix_pipeline_jobs_pipeline_stage_job",
                table: "pipeline_jobs",
                columns: new[] { "pipeline_id", "stage_seq", "job_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_template_jobs_pipeline_template_stage_id",
                table: "pipeline_template_jobs",
                column: "pipeline_template_stage_id");

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_template_stages_pipeline_template_id",
                table: "pipeline_template_stages",
                column: "pipeline_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_templates_ApplicationId",
                table: "pipeline_templates",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "ix_pipeline_templates_name",
                table: "pipeline_templates",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_templates_SubApplicationId",
                table: "pipeline_templates",
                column: "SubApplicationId");

            migrationBuilder.CreateIndex(
                name: "ix_pipeline_templates_template_key",
                table: "pipeline_templates",
                column: "template_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pipelines_app_key",
                table: "pipelines",
                column: "app_key");

            migrationBuilder.CreateIndex(
                name: "ix_pipelines_iteration_id",
                table: "pipelines",
                column: "iteration_id");

            migrationBuilder.CreateIndex(
                name: "IX_pipelines_pipeline_template_id",
                table: "pipelines",
                column: "pipeline_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_requirement_developers_user_id",
                table: "requirement_developers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_requirement_followers_user_id",
                table: "requirement_followers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sub_applications_app_key",
                table: "sub_applications",
                column: "app_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sub_applications_parent_application_id",
                table: "sub_applications",
                column: "parent_application_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_ding_talk_user_id",
                table: "users",
                column: "ding_talk_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_mobile",
                table: "users",
                column: "mobile",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_monitors");

            migrationBuilder.DropTable(
                name: "deploys");

            migrationBuilder.DropTable(
                name: "integration_release_apps");

            migrationBuilder.DropTable(
                name: "iteration_requirements");

            migrationBuilder.DropTable(
                name: "pipeline_jobs");

            migrationBuilder.DropTable(
                name: "pipeline_template_jobs");

            migrationBuilder.DropTable(
                name: "requirement_developers");

            migrationBuilder.DropTable(
                name: "requirement_followers");

            migrationBuilder.DropTable(
                name: "iterations");

            migrationBuilder.DropTable(
                name: "pipelines");

            migrationBuilder.DropTable(
                name: "pipeline_template_stages");

            migrationBuilder.DropTable(
                name: "requirements");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "integration_releases");

            migrationBuilder.DropTable(
                name: "pipeline_templates");

            migrationBuilder.DropTable(
                name: "sub_applications");

            migrationBuilder.DropTable(
                name: "applications");
        }
    }
}
