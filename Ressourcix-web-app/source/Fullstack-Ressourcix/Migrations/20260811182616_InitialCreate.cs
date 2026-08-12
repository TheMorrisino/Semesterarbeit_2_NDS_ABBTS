using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FullstackRessourcix.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Reference = table.Column<Guid>(type: "uuid", nullable: false),
                    Actor = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    workload = table.Column<int>(type: "integer", nullable: false),
                    vacationDays = table.Column<double>(type: "double precision", nullable: false),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    passwordHash = table.Column<string>(type: "text", nullable: false),
                    mustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    permissionLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    from = table.Column<DateOnly>(type: "date", nullable: false),
                    until = table.Column<DateOnly>(type: "date", nullable: false),
                    days = table.Column<int>(type: "integer", nullable: false),
                    overlap = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    submittedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    remark = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_requests_employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "employees",
                columns: new[] { "id", "isActive", "mustChangePassword", "name", "passwordHash", "permissionLevel", "role", "username", "vacationDays", "workload" },
                values: new object[,]
                {
                    { new Guid("144eda86-a7a2-419d-a37d-e16726e3828c"), false, true, "Tiago de Sousa Sá", "AQAAAAIAAYagAAAAEB9r8iV+OfZIu2V5H1x/Hh3SM3oPW8VHpRdMSxKV+bTtHl2pQleQPQzWZRZstzlw2w==", 1, "Mitarbeitende", "tiago.desousa", 3.2999999999999998, 60 },
                    { new Guid("4c3469de-428f-437e-b752-46f56714f063"), true, true, "Morris Meier", "AQAAAAIAAYagAAAAEB9r8iV+OfZIu2V5H1x/Hh3SM3oPW8VHpRdMSxKV+bTtHl2pQleQPQzWZRZstzlw2w==", 1, "Mitarbeitende", "morris.meier", 5.0, 100 },
                    { new Guid("77f37330-cb2b-4a5b-9f6a-6c2d19fde288"), true, true, "Lena Brunner", "AQAAAAIAAYagAAAAEB9r8iV+OfZIu2V5H1x/Hh3SM3oPW8VHpRdMSxKV+bTtHl2pQleQPQzWZRZstzlw2w==", 1, "Mitarbeitende", "lena.brunner", 4.4000000000000004, 80 },
                    { new Guid("86df2463-1bcd-42de-bb97-2cf112caeabf"), true, true, "Pedro Santos", "AQAAAAIAAYagAAAAEB9r8iV+OfZIu2V5H1x/Hh3SM3oPW8VHpRdMSxKV+bTtHl2pQleQPQzWZRZstzlw2w==", 5, "Planner/Leitung", "pedro.santos", 5.0, 100 }
                });

            migrationBuilder.InsertData(
                table: "requests",
                columns: new[] { "id", "days", "employeeId", "from", "overlap", "remark", "status", "submittedOn", "type", "until" },
                values: new object[] { new Guid("7e978e11-0a00-4e05-b61a-007763f529cd"), 10, new Guid("4c3469de-428f-437e-b752-46f56714f063"), new DateOnly(2026, 7, 13), true, null, 0, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), 0, new DateOnly(2026, 7, 24) });

            migrationBuilder.CreateIndex(
                name: "IX_employees_username",
                table: "employees",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_requests_employeeId",
                table: "requests",
                column: "employeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log_entries");

            migrationBuilder.DropTable(
                name: "requests");

            migrationBuilder.DropTable(
                name: "employees");
        }
    }
}
