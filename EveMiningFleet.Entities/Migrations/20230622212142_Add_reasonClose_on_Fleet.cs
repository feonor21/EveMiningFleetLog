using Microsoft.EntityFrameworkCore.Migrations;

namespace EveMiningFleet.Entities.Migrations
{
    public partial class Add_reasonClose_on_Fleet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReasonClose",
                table: "Fleets",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReasonClose",
                table: "Fleets");
        }
    }
}
