using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionOrdinateurs.API.Migrations
{
    /// <inheritdoc />
    public partial class RefactorComputerAndRepairHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reparations_Ordinateurs_OrdinateurId",
                table: "Reparations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ordinateurs",
                table: "Ordinateurs");

            migrationBuilder.RenameTable(
                name: "Ordinateurs",
                newName: "Ordinateur");

            migrationBuilder.Sql(
                "UPDATE `Ordinateur` SET " +
                "`TailleEcran` = COALESCE(`TailleEcran`, 0.0), " +
                "`PuissanceW` = COALESCE(`PuissanceW`, 0), " +
                "`CarteGraphique` = COALESCE(`CarteGraphique`, 0), " +
                "`AutonomieHeures` = COALESCE(`AutonomieHeures`, 0) " +
                "WHERE `TailleEcran` IS NULL " +
                "OR `PuissanceW` IS NULL " +
                "OR `CarteGraphique` IS NULL " +
                "OR `AutonomieHeures` IS NULL");

            migrationBuilder.Sql(
                "ALTER TABLE `Ordinateur` RENAME COLUMN `TailleEcran` " +
                "TO `Caracteristiques_TailleEcran`");
            migrationBuilder.Sql(
                "ALTER TABLE `Ordinateur` RENAME COLUMN `PuissanceW` " +
                "TO `Caracteristiques_PuissanceW`");
            migrationBuilder.Sql(
                "ALTER TABLE `Ordinateur` RENAME COLUMN `Processeur` " +
                "TO `Caracteristiques_Processeur`");
            migrationBuilder.Sql(
                "ALTER TABLE `Ordinateur` RENAME COLUMN `CarteGraphique` " +
                "TO `Caracteristiques_CarteGraphique`");
            migrationBuilder.Sql(
                "ALTER TABLE `Ordinateur` RENAME COLUMN `AutonomieHeures` " +
                "TO `Caracteristiques_AutonomieHeures`");
            migrationBuilder.Sql(
                "ALTER TABLE `Ordinateur` RENAME COLUMN `Ram` " +
                "TO `Caracteristiques_RamGo`");

            migrationBuilder.Sql(
                "ALTER TABLE `Reparations` ADD COLUMN `Technicien` longtext NULL");
            migrationBuilder.Sql(
                "UPDATE `Reparations` SET `Technicien` = '' WHERE `Technicien` IS NULL");
            migrationBuilder.Sql(
                "ALTER TABLE `Reparations` MODIFY `Technicien` longtext NOT NULL");

            migrationBuilder.AlterColumn<int>(
                name: "Stock",
                table: "Ordinateur",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<double>(
                name: "Caracteristiques_TailleEcran",
                table: "Ordinateur",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Caracteristiques_PuissanceW",
                table: "Ordinateur",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Caracteristiques_CarteGraphique",
                table: "Ordinateur",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Caracteristiques_AutonomieHeures",
                table: "Ordinateur",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Caracteristiques_TypeOrdi",
                table: "Ordinateur",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE `Ordinateur` SET `Caracteristiques_TypeOrdi` = " +
                "CASE `Discriminator` WHEN 'OrdinateurFixe' THEN 1 " +
                "WHEN 'OrdinateurPortable' THEN 0 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Ordinateur");

            migrationBuilder.AddColumn<int>(
                name: "Caracteristiques_StockageGo",
                table: "Ordinateur",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ordinateur",
                table: "Ordinateur",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reparations_Ordinateur_OrdinateurId",
                table: "Reparations",
                column: "OrdinateurId",
                principalTable: "Ordinateur",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reparations_Ordinateur_OrdinateurId",
                table: "Reparations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ordinateur",
                table: "Ordinateur");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Ordinateur",
                type: "varchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE `Ordinateur` SET `Discriminator` = " +
                "CASE `Caracteristiques_TypeOrdi` WHEN 1 THEN 'OrdinateurFixe' " +
                "ELSE 'OrdinateurPortable' END");

            migrationBuilder.DropColumn(
                name: "Technicien",
                table: "Reparations");

            migrationBuilder.DropColumn(
                name: "Caracteristiques_TypeOrdi",
                table: "Ordinateur");

            migrationBuilder.DropColumn(
                name: "Caracteristiques_StockageGo",
                table: "Ordinateur");

            migrationBuilder.RenameTable(
                name: "Ordinateur",
                newName: "Ordinateurs");

            migrationBuilder.RenameColumn(
                name: "Caracteristiques_TailleEcran",
                table: "Ordinateurs",
                newName: "TailleEcran");

            migrationBuilder.RenameColumn(
                name: "Caracteristiques_PuissanceW",
                table: "Ordinateurs",
                newName: "PuissanceW");

            migrationBuilder.RenameColumn(
                name: "Caracteristiques_Processeur",
                table: "Ordinateurs",
                newName: "Processeur");

            migrationBuilder.RenameColumn(
                name: "Caracteristiques_CarteGraphique",
                table: "Ordinateurs",
                newName: "CarteGraphique");

            migrationBuilder.RenameColumn(
                name: "Caracteristiques_AutonomieHeures",
                table: "Ordinateurs",
                newName: "AutonomieHeures");

            migrationBuilder.RenameColumn(
                name: "Caracteristiques_RamGo",
                table: "Ordinateurs",
                newName: "Ram");

            migrationBuilder.AlterColumn<int>(
                name: "Stock",
                table: "Ordinateurs",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "TailleEcran",
                table: "Ordinateurs",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<int>(
                name: "PuissanceW",
                table: "Ordinateurs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "CarteGraphique",
                table: "Ordinateurs",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<int>(
                name: "AutonomieHeures",
                table: "Ordinateurs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ordinateurs",
                table: "Ordinateurs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reparations_Ordinateurs_OrdinateurId",
                table: "Reparations",
                column: "OrdinateurId",
                principalTable: "Ordinateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
