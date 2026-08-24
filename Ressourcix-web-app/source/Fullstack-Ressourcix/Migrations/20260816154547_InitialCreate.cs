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
          Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_audit_log_entries", x => x.Id);
        }
      );

      migrationBuilder.CreateTable(
        name: "employees",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          Name = table.Column<string>(type: "text", nullable: false),
          Role = table.Column<string>(type: "text", nullable: false),
          Workload = table.Column<int>(type: "integer", nullable: false),
          VacationDays = table.Column<double>(type: "double precision", nullable: false),
          IsActive = table.Column<bool>(type: "boolean", nullable: false),
          Username = table.Column<string>(type: "text", nullable: false),
          PasswordHash = table.Column<string>(type: "text", nullable: false),
          MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
          PermissionLevel = table.Column<int>(type: "integer", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_employees", x => x.Id);
        }
      );

      migrationBuilder.CreateTable(
        name: "requests",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
          From = table.Column<DateOnly>(type: "date", nullable: false),
          Until = table.Column<DateOnly>(type: "date", nullable: false),
          Days = table.Column<int>(type: "integer", nullable: false),
          Overlap = table.Column<bool>(type: "boolean", nullable: false),
          Status = table.Column<int>(type: "integer", nullable: false),
          SubmittedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
          Type = table.Column<int>(type: "integer", nullable: false),
          Remark = table.Column<string>(type: "text", nullable: true),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_requests", x => x.Id);
          table.ForeignKey(
            name: "FK_requests_employees_EmployeeId",
            column: x => x.EmployeeId,
            principalTable: "employees",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.InsertData(
        table: "employees",
        columns: new[]
        {
          "Id",
          "IsActive",
          "MustChangePassword",
          "Name",
          "PasswordHash",
          "PermissionLevel",
          "Role",
          "Username",
          "VacationDays",
          "Workload",
        },
        values: new object[,]
        {
          {
            new Guid("144eda86-a7a2-419d-a37d-e16726e3828c"),
            false,
            true,
            "Tiago de Sousa Sá",
            "AQAAAAIAAYagAAAAEB9r8iV+OfZIu2V5H1x/Hh3SM3oPW8VHpRdMSxKV+bTtHl2pQleQPQzWZRZstzlw2w==",
            1,
            "Mitarbeiter",
            "tiago.desousa",
            16.5,
            60,
          },
          {
            new Guid("4c3469de-428f-437e-b752-46f56714f063"),
            true,
            true,
            "Morris Meier",
            "AQAAAAIAAYagAAAAEB9r8iV+OfZIu2V5H1x/Hh3SM3oPW8VHpRdMSxKV+bTtHl2pQleQPQzWZRZstzlw2w==",
            1,
            "Mitarbeiter",
            "morris.meier",
            25.0,
            100,
          },
          {
            new Guid("77f37330-cb2b-4a5b-9f6a-6c2d19fde288"),
            true,
            true,
            "Lena Brunner",
            "AQAAAAIAAYagAAAAEB9r8iV+OfZIu2V5H1x/Hh3SM3oPW8VHpRdMSxKV+bTtHl2pQleQPQzWZRZstzlw2w==",
            1,
            "Mitarbeiter",
            "lena.brunner",
            22.0,
            80,
          },
          {
            new Guid("86df2463-1bcd-42de-bb97-2cf112caeabf"),
            true,
            true,
            "Pedro Santos",
            "AQAAAAIAAYagAAAAEB9r8iV+OfZIu2V5H1x/Hh3SM3oPW8VHpRdMSxKV+bTtHl2pQleQPQzWZRZstzlw2w==",
            5,
            "Planer/Leitung",
            "pedro.santos",
            25.0,
            100,
          },
          {
            new Guid("13dbff56-5437-4b8d-94f0-cd7384730134"),
            true,
            false,
            "Admin Test",
            "AQAAAAIAAYagAAAAEOz6Rz8DlXt/pW8KjR44w3LhUBdip5dHpKCNsOS/lJeNKk5hMdRlPz/V+v7MhFSy5Q==",
            5,
            "Planer/Leitung",
            "admin",
            25.0,
            100,
          },
        }
      );

      migrationBuilder.InsertData(
        table: "requests",
        columns: new[]
        {
          "Id",
          "Days",
          "EmployeeId",
          "From",
          "Overlap",
          "Remark",
          "Status",
          "SubmittedOn",
          "Type",
          "Until",
        },
        values: new object[]
        {
          new Guid("7e978e11-0a00-4e05-b61a-007763f529cd"),
          10,
          new Guid("4c3469de-428f-437e-b752-46f56714f063"),
          new DateOnly(2026, 7, 13),
          true,
          null,
          0,
          new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc),
          0,
          new DateOnly(2026, 7, 24),
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_employees_Username",
        table: "employees",
        column: "Username",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_requests_EmployeeId",
        table: "requests",
        column: "EmployeeId"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "audit_log_entries");

      migrationBuilder.DropTable(name: "requests");

      migrationBuilder.DropTable(name: "employees");
    }
  }
}
