using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataProcessor.Service.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sensor_readings",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sensor_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    energy_consumption = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    co2 = table.Column<int>(type: "integer", nullable: true),
                    pm25 = table.Column<int>(type: "integer", nullable: true),
                    humidity = table.Column<int>(type: "integer", nullable: true),
                    motion_detected = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensor_readings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_sensor_readings_sensor_id",
                table: "sensor_readings",
                column: "sensor_id");

            migrationBuilder.CreateIndex(
                name: "idx_sensor_readings_timestamp",
                table: "sensor_readings",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "idx_sensor_readings_type",
                table: "sensor_readings",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sensor_readings");
        }
    }
}
