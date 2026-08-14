using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FullstackRessourcix.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVacationDaysToDaysUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("144eda86-a7a2-419d-a37d-e16726e3828c"),
                column: "vacationDays",
                value: 16.5);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("4c3469de-428f-437e-b752-46f56714f063"),
                column: "vacationDays",
                value: 25.0);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("77f37330-cb2b-4a5b-9f6a-6c2d19fde288"),
                column: "vacationDays",
                value: 22.0);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("86df2463-1bcd-42de-bb97-2cf112caeabf"),
                column: "vacationDays",
                value: 25.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("144eda86-a7a2-419d-a37d-e16726e3828c"),
                column: "vacationDays",
                value: 3.2999999999999998);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("4c3469de-428f-437e-b752-46f56714f063"),
                column: "vacationDays",
                value: 5.0);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("77f37330-cb2b-4a5b-9f6a-6c2d19fde288"),
                column: "vacationDays",
                value: 4.4000000000000004);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("86df2463-1bcd-42de-bb97-2cf112caeabf"),
                column: "vacationDays",
                value: 5.0);
        }
    }
}
