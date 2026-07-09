using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameMustChangePasswordToForcePasswordChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MustChangePassword",
                table: "AspNetUsers",
                newName: "ForcePasswordChange");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ForcePasswordChange",
                table: "AspNetUsers",
                newName: "MustChangePassword");
        }
    }
}
