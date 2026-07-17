using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TmsApi.Migrations
{
    /// <inheritdoc />
    public partial class addCertificateConfigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificate_Courses_CourseId",
                table: "Certificate");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificate_Students_StudentId",
                table: "Certificate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Certificate",
                table: "Certificate");

            migrationBuilder.RenameTable(
                name: "Certificate",
                newName: "Certificat");

            migrationBuilder.RenameIndex(
                name: "IX_Certificate_StudentId",
                table: "Certificat",
                newName: "IX_Certificat_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Certificate_CourseId",
                table: "Certificat",
                newName: "IX_Certificat_CourseId");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Certificat",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Certificat",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Certificat",
                table: "Certificat",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Certificat_SerialNumber",
                table: "Certificat",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificat_Courses_CourseId",
                table: "Certificat",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificat_Students_StudentId",
                table: "Certificat",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificat_Courses_CourseId",
                table: "Certificat");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificat_Students_StudentId",
                table: "Certificat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Certificat",
                table: "Certificat");

            migrationBuilder.DropIndex(
                name: "IX_Certificat_SerialNumber",
                table: "Certificat");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Certificat");

            migrationBuilder.RenameTable(
                name: "Certificat",
                newName: "Certificate");

            migrationBuilder.RenameIndex(
                name: "IX_Certificat_StudentId",
                table: "Certificate",
                newName: "IX_Certificate_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Certificat_CourseId",
                table: "Certificate",
                newName: "IX_Certificate_CourseId");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Certificate",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Certificate",
                table: "Certificate",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificate_Courses_CourseId",
                table: "Certificate",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificate_Students_StudentId",
                table: "Certificate",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
