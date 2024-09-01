using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testproject.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "users",
                newName: "PasswordHash");

            migrationBuilder.AddColumn<byte[]>(
                name: "Salt",
                table: "users",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salt",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "Password");
        }
    }
}
