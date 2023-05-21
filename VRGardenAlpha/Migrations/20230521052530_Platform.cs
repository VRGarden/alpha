using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRGardenAlpha.Migrations
{
    /// <inheritdoc />
    public partial class Platform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Platform",
                table: "Posts",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Posts");
        }
    }
}
