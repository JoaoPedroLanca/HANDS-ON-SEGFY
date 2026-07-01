using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesafioSegfy.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaTabelaApolice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apolice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NumeroApolice = table.Column<string>(type: "TEXT", nullable: false),
                    CpfCnpj = table.Column<string>(type: "TEXT", nullable: false),
                    Placa = table.Column<string>(type: "TEXT", nullable: false),
                    ValorPremio = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataIncioVigencia = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataFimVigencia = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apolice", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apolice_CpfCnpj",
                table: "Apolice",
                column: "CpfCnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Apolice_NumeroApolice",
                table: "Apolice",
                column: "NumeroApolice",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Apolice");
        }
    }
}
