using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZeiterfassungWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalIdToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonalId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonalId",
                table: "AspNetUsers");
        }
    }
}
