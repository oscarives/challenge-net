using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosMedicos.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyPacienteFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bloqueado",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "NoShowCount",
                table: "Pacientes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Bloqueado",
                table: "Pacientes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NoShowCount",
                table: "Pacientes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Pacientes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Bloqueado", "NoShowCount" },
                values: new object[] { false, 0 });

            migrationBuilder.UpdateData(
                table: "Pacientes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Bloqueado", "NoShowCount" },
                values: new object[] { false, 0 });

            migrationBuilder.UpdateData(
                table: "Pacientes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Bloqueado", "NoShowCount" },
                values: new object[] { false, 0 });

            migrationBuilder.UpdateData(
                table: "Pacientes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Bloqueado", "NoShowCount" },
                values: new object[] { false, 0 });

            migrationBuilder.UpdateData(
                table: "Pacientes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Bloqueado", "NoShowCount" },
                values: new object[] { false, 0 });

            migrationBuilder.UpdateData(
                table: "Pacientes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Bloqueado", "NoShowCount" },
                values: new object[] { false, 0 });

            migrationBuilder.UpdateData(
                table: "Pacientes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Bloqueado", "NoShowCount" },
                values: new object[] { true, 0 });

            migrationBuilder.UpdateData(
                table: "Pacientes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Bloqueado", "NoShowCount" },
                values: new object[] { false, 0 });
        }
    }
}
