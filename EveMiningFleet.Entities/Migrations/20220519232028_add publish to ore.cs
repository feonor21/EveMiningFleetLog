using Microsoft.EntityFrameworkCore.Migrations;

namespace EveMiningFleet.Entities.Migrations
{
    public partial class addpublishtoore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Publish",
                table: "Ores",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Publish",
                table: "Ores");
        }
    }
}
