using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitorServidores.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEliminadoCatalogosIncidente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "TiposIncidente",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "PrioridadesIncidente",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "EstadosIncidente",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "TiposIncidente");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "PrioridadesIncidente");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "EstadosIncidente");
        }
    }
}
