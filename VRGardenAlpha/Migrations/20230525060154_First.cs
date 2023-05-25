using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VRGardenAlpha.Data;

#nullable disable

namespace VRGardenAlpha.Migrations
{
    /// <inheritdoc />
    public partial class First : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    Downloads = table.Column<int>(type: "integer", nullable: false),
                    ACL = table.Column<int>(type: "integer", nullable: false),
                    Platform = table.Column<byte>(type: "smallint", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Author = table.Column<string>(type: "text", nullable: false),
                    AuthorIP = table.Column<IPAddress>(type: "inet", nullable: false),
                    RemoteId = table.Column<string>(type: "text", nullable: true),
                    ContentLink = table.Column<string>(type: "text", nullable: true),
                    Creator = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Checksum = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    ContentLength = table.Column<long>(type: "bigint", nullable: false),
                    ImageContentType = table.Column<string>(type: "text", nullable: false),
                    ImageContentLength = table.Column<long>(type: "bigint", nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    Features = table.Column<List<string>>(type: "text[]", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastChunk = table.Column<int>(type: "integer", nullable: false),
                    Chunks = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

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
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
