using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EveMiningFleet.Entities.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlerteMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(nullable: true),
                    End = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                });

            migrationBuilder.CreateTable(
                name: "Alliances",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                });

            migrationBuilder.CreateTable(
                name: "Corporations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                });

            migrationBuilder.CreateTable(
                name: "Dataprices",
                columns: table => new
                {
                    TypeId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PriceSell = table.Column<double>(nullable: false),
                    PriceBuy = table.Column<double>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TypeId)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                });

            migrationBuilder.CreateTable(
                name: "Invtypematerials",
                columns: table => new
                {
                    TypeId = table.Column<int>(nullable: false),
                    MaterialTypeId = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.TypeId, x.MaterialTypeId })
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                });

            migrationBuilder.CreateTable(
                name: "Ores",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Volume = table.Column<float>(nullable: true),
                    CanBeCompressed = table.Column<bool>(nullable: true),
                    QuantityForReprocess = table.Column<int>(nullable: true),
                    IdCompressed = table.Column<int>(nullable: true),
                    IdSkillOreReprocessing = table.Column<int>(nullable: true),
                    PriceCompressedBuy = table.Column<double>(nullable: true),
                    PriceCompressedSell = table.Column<double>(nullable: true),
                    PriceRefinedBuy = table.Column<double>(nullable: true),
                    PriceRefinedSell = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Token = table.Column<string>(nullable: true),
                    RefreshToken = table.Column<string>(nullable: true),
                    CorporationId = table.Column<int>(nullable: false),
                    AllianceId = table.Column<int>(nullable: false),
                    CharacterMainId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_Characters_Alliances_AllianceId",
                        column: x => x.AllianceId,
                        principalTable: "Alliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Characters_Characters_CharacterMainId",
                        column: x => x.CharacterMainId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Characters_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fleets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Begin = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: true),
                    JoinToken = table.Column<string>(nullable: true),
                    LastFullRefresh = table.Column<DateTime>(nullable: true),
                    ViewRight = table.Column<int>(nullable: true),
                    Distribution = table.Column<int>(nullable: true),
                    Reprocess = table.Column<double>(nullable: true),
                    CharacterId = table.Column<int>(nullable: false),
                    CorporationId = table.Column<int>(nullable: false),
                    AllianceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_Fleets_Alliances_AllianceId",
                        column: x => x.AllianceId,
                        principalTable: "Alliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fleets_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fleets_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lastmininglogs",
                columns: table => new
                {
                    CharacterId = table.Column<int>(nullable: false),
                    OreId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Quantity = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.CharacterId, x.OreId, x.Date })
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });
                    table.ForeignKey(
                        name: "FK_lastMiningLogs_characters_Character_Id",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lastMiningLogs_ores_Ore_Id",
                        column: x => x.OreId,
                        principalTable: "Ores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fleetcharacters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FleetId = table.Column<int>(nullable: false),
                    CharacterId = table.Column<int>(nullable: false),
                    Join = table.Column<DateTime>(nullable: false),
                    Quit = table.Column<DateTime>(nullable: true),
                    LastRefresh = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_Fleetcharacters_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fleetcharacters_Fleets_FleetId",
                        column: x => x.FleetId,
                        principalTable: "Fleets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fleetgroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FleetId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_Fleetgroups_Fleets_FleetId",
                        column: x => x.FleetId,
                        principalTable: "Fleets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fleettaxes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FleetId = table.Column<int>(nullable: false),
                    CharacterId = table.Column<int>(nullable: true),
                    Taxe = table.Column<float>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_Fleettaxes_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fleettaxes_Fleets_FleetId",
                        column: x => x.FleetId,
                        principalTable: "Fleets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mininglogs",
                columns: table => new
                {
                    FleetCharacterId = table.Column<int>(nullable: false),
                    OreId = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.FleetCharacterId, x.OreId })
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_Mininglogs_Fleetcharacters_FleetCharacterId",
                        column: x => x.FleetCharacterId,
                        principalTable: "Fleetcharacters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mininglogs_Ores_OreId",
                        column: x => x.OreId,
                        principalTable: "Ores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fleetgroupcharacters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FleetgroupId = table.Column<int>(nullable: false),
                    CharacterId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id)
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_Fleetgroupcharacters_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fleetgroupcharacters_Fleetgroups_FleetgroupId",
                        column: x => x.FleetgroupId,
                        principalTable: "Fleetgroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_AllianceId",
                table: "Characters",
                column: "AllianceId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_CharacterMainId",
                table: "Characters",
                column: "CharacterMainId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_CorporationId",
                table: "Characters",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleetcharacters_CharacterId",
                table: "Fleetcharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleetcharacters_FleetId",
                table: "Fleetcharacters",
                column: "FleetId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleetgroupcharacters_CharacterId",
                table: "Fleetgroupcharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleetgroupcharacters_FleetgroupId",
                table: "Fleetgroupcharacters",
                column: "FleetgroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleetgroups_FleetId",
                table: "Fleetgroups",
                column: "FleetId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleets_AllianceId",
                table: "Fleets",
                column: "AllianceId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleets_CharacterId",
                table: "Fleets",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleets_CorporationId",
                table: "Fleets",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleettaxes_CharacterId",
                table: "Fleettaxes",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Fleettaxes_FleetId",
                table: "Fleettaxes",
                column: "FleetId");

            migrationBuilder.CreateIndex(
                name: "IX_Lastmininglogs_OreId",
                table: "Lastmininglogs",
                column: "OreId");

            migrationBuilder.CreateIndex(
                name: "IX_Mininglogs_OreId",
                table: "Mininglogs",
                column: "OreId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlerteMessages");

            migrationBuilder.DropTable(
                name: "Dataprices");

            migrationBuilder.DropTable(
                name: "Fleetgroupcharacters");

            migrationBuilder.DropTable(
                name: "Fleettaxes");

            migrationBuilder.DropTable(
                name: "Invtypematerials");

            migrationBuilder.DropTable(
                name: "Lastmininglogs");

            migrationBuilder.DropTable(
                name: "Mininglogs");

            migrationBuilder.DropTable(
                name: "Fleetgroups");

            migrationBuilder.DropTable(
                name: "Fleetcharacters");

            migrationBuilder.DropTable(
                name: "Ores");

            migrationBuilder.DropTable(
                name: "Fleets");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Alliances");

            migrationBuilder.DropTable(
                name: "Corporations");
        }
    }
}
