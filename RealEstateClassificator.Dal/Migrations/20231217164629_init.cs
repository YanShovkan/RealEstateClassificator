using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateClassificator.Dal.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Card",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    District = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: true),
                    Floors = table.Column<int>(type: "integer", nullable: true),
                    Rooms = table.Column<int>(type: "integer", nullable: true),
                    TotalArea = table.Column<double>(type: "double precision", nullable: true),
                    LivingArea = table.Column<double>(type: "double precision", nullable: true),
                    KitchenArea = table.Column<double>(type: "double precision", nullable: true),
                    Renovation = table.Column<int>(type: "integer", nullable: true),
                    CombinedBathrooms = table.Column<int>(type: "integer", nullable: true),
                    SeparateBathrooms = table.Column<int>(type: "integer", nullable: true),
                    BalconiesCount = table.Column<int>(type: "integer", nullable: true),
                    DistanceToCity = table.Column<double>(type: "double precision", nullable: true),
                    BuiltYear = table.Column<int>(type: "integer", nullable: true),
                    PassengerLiftsCount = table.Column<int>(type: "integer", nullable: true),
                    CargoLiftsCount = table.Column<int>(type: "integer", nullable: true),
                    IsStudio = table.Column<bool>(type: "boolean", nullable: true),
                    ClassOfCard = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Card");
        }
    }
}
