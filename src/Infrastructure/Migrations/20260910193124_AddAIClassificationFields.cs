using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAIClassificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NivelReportadoUsuario",
                table: "Reportes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Origen",
                table: "Analisis",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMensaje",
                table: "Analisis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Justificacion",
                table: "Analisis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NivelRiesgo",
                table: "Analisis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prioridad",
                table: "Analisis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecomendacionesJson",
                table: "Analisis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoRiesgo",
                table: "Analisis",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NivelReportadoUsuario",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "ErrorMensaje",
                table: "Analisis");

            migrationBuilder.DropColumn(
                name: "Justificacion",
                table: "Analisis");

            migrationBuilder.DropColumn(
                name: "NivelRiesgo",
                table: "Analisis");

            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "Analisis");

            migrationBuilder.DropColumn(
                name: "RecomendacionesJson",
                table: "Analisis");

            migrationBuilder.DropColumn(
                name: "TipoRiesgo",
                table: "Analisis");

            migrationBuilder.AlterColumn<string>(
                name: "Origen",
                table: "Analisis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
