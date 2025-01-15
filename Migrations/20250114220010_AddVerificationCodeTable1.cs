using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservationFrontOffice.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationCodeTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VerificationCode",
                table: "VerificationCode");

            migrationBuilder.RenameTable(
                name: "VerificationCode",
                newName: "VerificationCodes");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "VerificationCodes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VerificationCodes",
                table: "VerificationCodes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_Email",
                table: "VerificationCodes",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VerificationCodes",
                table: "VerificationCodes");

            migrationBuilder.DropIndex(
                name: "IX_VerificationCodes_Email",
                table: "VerificationCodes");

            migrationBuilder.RenameTable(
                name: "VerificationCodes",
                newName: "VerificationCode");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "VerificationCode",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VerificationCode",
                table: "VerificationCode",
                column: "Id");
        }
    }
}
