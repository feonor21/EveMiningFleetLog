using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EveMiningFleet.Entities.Migrations
{
    public partial class addusagehistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsageHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    date = table.Column<DateTime>(nullable: false),
                    fleetactive = table.Column<int>(nullable: true),
                    characteractif = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsageHistory_date",
                table: "UsageHistory",
                column: "date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsageHistory");
        }
    }
}
