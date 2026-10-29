using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace GestionOrdinateurs.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Ordinateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "longtext", nullable: false),
                    Marque = table.Column<string>(type: "longtext", nullable: false),
                    Ram = table.Column<int>(type: "int", nullable: false),
                    Processeur = table.Column<string>(type: "longtext", nullable: false),
                    Etat = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: false),
                    PuissanceW = table.Column<int>(type: "int", nullable: true),
                    CarteGraphique = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    TailleEcran = table.Column<double>(type: "double", nullable: true),
                    AutonomieHeures = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordinateurs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ordinateurs");
        }
    }
}
