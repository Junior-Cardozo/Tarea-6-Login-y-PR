using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MonitorServidores.Migrations
{
    /// <inheritdoc />
    public partial class IncidentesIniciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstadosIncidente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosIncidente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrioridadesIncidente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrioridadesIncidente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposIncidente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposIncidente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    ServidorId = table.Column<int>(type: "integer", nullable: false),
                    TipoIncidenteId = table.Column<int>(type: "integer", nullable: false),
                    PrioridadIncidenteId = table.Column<int>(type: "integer", nullable: false),
                    EstadoIncidenteId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidentes_EstadosIncidente_EstadoIncidenteId",
                        column: x => x.EstadoIncidenteId,
                        principalTable: "EstadosIncidente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidentes_PrioridadesIncidente_PrioridadIncidenteId",
                        column: x => x.PrioridadIncidenteId,
                        principalTable: "PrioridadesIncidente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidentes_TiposIncidente_TipoIncidenteId",
                        column: x => x.TipoIncidenteId,
                        principalTable: "TiposIncidente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidentes_servidores_ServidorId",
                        column: x => x.ServidorId,
                        principalTable: "servidores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_EstadoIncidenteId",
                table: "Incidentes",
                column: "EstadoIncidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_PrioridadIncidenteId",
                table: "Incidentes",
                column: "PrioridadIncidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_ServidorId",
                table: "Incidentes",
                column: "ServidorId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_TipoIncidenteId",
                table: "Incidentes",
                column: "TipoIncidenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Incidentes");

            migrationBuilder.DropTable(
                name: "EstadosIncidente");

            migrationBuilder.DropTable(
                name: "PrioridadesIncidente");

            migrationBuilder.DropTable(
                name: "TiposIncidente");
        }
    }
}
