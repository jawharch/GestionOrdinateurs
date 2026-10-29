using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace GestionOrdinateurs.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingStockAndRepairs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Prix",
                table: "Ordinateurs",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Ordinateurs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Reparations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrdinateurId = table.Column<int>(type: "int", nullable: false),
                    TypeReparation = table.Column<string>(type: "longtext", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Statut = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reparations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reparations_Ordinateurs_OrdinateurId",
                        column: x => x.OrdinateurId,
                        principalTable: "Ordinateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Reparations_OrdinateurId",
                table: "Reparations",
                column: "OrdinateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reparations");

            migrationBuilder.DropColumn(
                name: "Prix",
                table: "Ordinateurs");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Ordinateurs");
        }
    }
}
