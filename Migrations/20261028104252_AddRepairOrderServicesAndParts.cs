using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageAI.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairOrderServicesAndParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepairOrderParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RepairOrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairOrderParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairOrderParts_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RepairOrderServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RepairOrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    GarageServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairOrderServices_GarageServices_GarageServiceId",
                        column: x => x.GarageServiceId,
                        principalTable: "GarageServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairOrderServices_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderParts_PartId",
                table: "RepairOrderParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderParts_RepairOrderId",
                table: "RepairOrderParts",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderServices_GarageServiceId",
                table: "RepairOrderServices",
                column: "GarageServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderServices_RepairOrderId",
                table: "RepairOrderServices",
                column: "RepairOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairOrderParts");

            migrationBuilder.DropTable(
                name: "RepairOrderServices");
        }
    }
}
