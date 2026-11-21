using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MonitorServidores.Migrations
{
    /// <inheritdoc />
    public partial class InicialTablasYasmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sistemas_operativos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sistemas_operativos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_servidor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_servidor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "servidores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DireccionIp = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoServidorId = table.Column<int>(type: "integer", nullable: true),
                    SistemaOperativoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servidores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_servidores_sistemas_operativos_SistemaOperativoId",
                        column: x => x.SistemaOperativoId,
                        principalTable: "sistemas_operativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_servidores_tipos_servidor_TipoServidorId",
                        column: x => x.TipoServidorId,
                        principalTable: "tipos_servidor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "monitoreos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServidorId = table.Column<int>(type: "integer", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Disponible = table.Column<bool>(type: "boolean", nullable: false),
                    TiempoRespuestaMs = table.Column<int>(type: "integer", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitoreos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_monitoreos_servidores_ServidorId",
                        column: x => x.ServidorId,
                        principalTable: "servidores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_monitoreos_ServidorId",
                table: "monitoreos",
                column: "ServidorId");

            migrationBuilder.CreateIndex(
                name: "IX_servidores_SistemaOperativoId",
                table: "servidores",
                column: "SistemaOperativoId");

            migrationBuilder.CreateIndex(
                name: "IX_servidores_TipoServidorId",
                table: "servidores",
                column: "TipoServidorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "monitoreos");

            migrationBuilder.DropTable(
                name: "servidores");

            migrationBuilder.DropTable(
                name: "sistemas_operativos");

            migrationBuilder.DropTable(
                name: "tipos_servidor");
        }
    }
}
