using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FocusFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "focus_projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    updated_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    completed_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: true),
                    archived_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_focus_projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pomodoro_presets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    cycle_settings_json = table.Column<string>(type: "TEXT", nullable: false),
                    kind = table.Column<int>(type: "INTEGER", nullable: false),
                    auto_start_breaks = table.Column<bool>(type: "INTEGER", nullable: false),
                    auto_start_focus_sessions = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    updated_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pomodoro_presets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "focus_tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    estimated_pomodoros = table.Column<int>(type: "INTEGER", nullable: false),
                    project_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    updated_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    started_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: true),
                    completed_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: true),
                    cancelled_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: true),
                    completed_focus_session_ids_json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_focus_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_focus_tasks_focus_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "focus_projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "focus_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    task_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    preset_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    completion_reason = table.Column<int>(type: "INTEGER", nullable: false),
                    planned_duration_ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    remaining_duration_ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    actual_duration_ticks = table.Column<long>(type: "INTEGER", nullable: true),
                    started_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    expected_end_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: true),
                    paused_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: true),
                    completed_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: true),
                    cancelled_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: true),
                    created_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    updated_at_utc_ticks = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_focus_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_focus_sessions_focus_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "focus_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_focus_sessions_pomodoro_presets_preset_id",
                        column: x => x.preset_id,
                        principalTable: "pomodoro_presets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_focus_projects_status",
                table: "focus_projects",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_focus_sessions_preset_id",
                table: "focus_sessions",
                column: "preset_id");

            migrationBuilder.CreateIndex(
                name: "IX_focus_sessions_status",
                table: "focus_sessions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_focus_sessions_task_id",
                table: "focus_sessions",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_focus_tasks_project_id",
                table: "focus_tasks",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_focus_tasks_status",
                table: "focus_tasks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_pomodoro_presets_kind",
                table: "pomodoro_presets",
                column: "kind");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "focus_sessions");

            migrationBuilder.DropTable(
                name: "focus_tasks");

            migrationBuilder.DropTable(
                name: "pomodoro_presets");

            migrationBuilder.DropTable(
                name: "focus_projects");
        }
    }
}
