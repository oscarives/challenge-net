using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TurnosMedicos.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchemaNoShow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreCompleto = table.Column<string>(type: "TEXT", nullable: false),
                    DNI = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: false),
                    NoShowCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Bloqueado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaBloqueo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    isActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreCompleto = table.Column<string>(type: "TEXT", nullable: false),
                    Especialidad = table.Column<string>(type: "TEXT", nullable: false),
                    SucursalId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicos_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PacienteId = table.Column<int>(type: "INTEGER", nullable: true),
                    MedicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", nullable: false),
                    AusenciaPenalizada = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turnos_Medicos_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Turnos_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Pacientes",
                columns: new[] { "Id", "Bloqueado", "DNI", "Email", "FechaBloqueo", "NoShowCount", "NombreCompleto", "Telefono", "createdAt", "isActive" },
                values: new object[,]
                {
                    { 1, false, "28445123", "gonzalo.herrera@gmail.com", null, 0, "Gonzalo Herrera", "1145678901", new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 2, false, "31202456", "mariana.lopez@hotmail.com", null, 0, "Mariana López", "1167891234", new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 3, false, "25789012", "roberto.q@gmail.com", null, 0, "Roberto Quintana", "1156789012", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 4, false, "35123789", "sofia.vargas@yahoo.com", null, 0, "Sofía Vargas", "1178901234", new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 5, false, "29876543", "diegofernandez@gmail.com", null, 0, "Diego Fernández", "1134567890", new DateTime(2026, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 6, false, "33456789", "lauragimenez@gmail.com", null, 0, "Laura Giménez", "1189012345", new DateTime(2026, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 7, true, "27345678", "facundo.sosa@outlook.com", new DateTime(2026, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, "Facundo Sosa", "1112345678", new DateTime(2025, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 8, false, "36901234", "carolina.mendez@gmail.com", null, 0, "Carolina Méndez", "1190123456", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), true }
                });

            migrationBuilder.InsertData(
                table: "Sucursales",
                columns: new[] { "Id", "Direccion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Av. Santa Fe 3500, CABA", "Palermo" },
                    { 2, "Av. Cabildo 1200, CABA", "Belgrano" },
                    { 3, "Defensa 850, CABA", "San Telmo" }
                });

            migrationBuilder.InsertData(
                table: "Medicos",
                columns: new[] { "Id", "Especialidad", "NombreCompleto", "SucursalId" },
                values: new object[,]
                {
                    { 1, "Clínica Médica", "Dr. Alejandro Ríos", 1 },
                    { 2, "Pediatría", "Dra. Valentina Mora", 1 },
                    { 3, "Cardiología", "Dr. Carlos Benítez", 2 },
                    { 4, "Dermatología", "Dra. Lucía Ferraro", 2 },
                    { 5, "Traumatología", "Dr. Martín Gutiérrez", 3 }
                });

            migrationBuilder.InsertData(
                table: "Turnos",
                columns: new[] { "Id", "AusenciaPenalizada", "Estado", "FechaCreacion", "FechaHora", "MedicoId", "Motivo", "PacienteId" },
                values: new object[,]
                {
                    { 1, false, "Pendiente", new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 5, 9, 0, 0, 0, DateTimeKind.Unspecified), 1, "Control anual", 1 },
                    { 2, false, "Confirmado", new DateTime(2026, 4, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 30, 11, 0, 0, 0, DateTimeKind.Unspecified), 3, "Revisión cardiológica", 2 },
                    { 3, false, "NoShow", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 3, 14, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, "Pediatría seguimiento", 2 },
                    { 4, false, "NoShow", new DateTime(2026, 2, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 2, 18, 9, 0, 0, 0, DateTimeKind.Unspecified), 1, "Consulta general", 2 },
                    { 5, false, "Pendiente", new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 12, 15, 0, 0, 0, DateTimeKind.Unspecified), 4, "Revisión de piel", 3 },
                    { 6, false, "Confirmado", new DateTime(2026, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 20, 14, 0, 0, 0, DateTimeKind.Unspecified), 5, "Dolor en rodilla derecha", 4 },
                    { 7, false, "Cancelado", new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 10, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, "Chequeo general", 5 },
                    { 8, false, "Pendiente", new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), 2, "Control pediátrico", 6 },
                    { 9, false, "Atendido", new DateTime(2026, 3, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 11, 0, 0, 0, DateTimeKind.Unspecified), 3, "Electrocardiograma de rutina", 1 },
                    { 10, false, "Pendiente", new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 3, 16, 0, 0, 0, DateTimeKind.Unspecified), 4, "Primera consulta", 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_SucursalId",
                table: "Medicos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_MedicoId",
                table: "Turnos",
                column: "MedicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_PacienteId",
                table: "Turnos",
                column: "PacienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Turnos");

            migrationBuilder.DropTable(
                name: "Medicos");

            migrationBuilder.DropTable(
                name: "Pacientes");

            migrationBuilder.DropTable(
                name: "Sucursales");
        }
    }
}
