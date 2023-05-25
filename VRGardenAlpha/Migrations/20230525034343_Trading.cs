using System;
using Microsoft.EntityFrameworkCore.Migrations;
using VRGardenAlpha.Data;

#nullable disable

namespace VRGardenAlpha.Migrations
{
    /// <inheritdoc />
    public partial class Trading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ACL = table.Column<int>(type: "integer", nullable: false),
                    Initiator = table.Column<TradeDetails>(type: "jsonb", nullable: false),
                    Recipient = table.Column<TradeDetails>(type: "jsonb", nullable: false),
                    InitiatorPaths = table.Column<string[]>(type: "text[]", nullable: true),
                    RecipientPaths = table.Column<string[]>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
