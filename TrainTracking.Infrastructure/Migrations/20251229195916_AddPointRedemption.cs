using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPointRedemption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PointRedemptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    PointsRedeemed = table.Column<int>(type: "INTEGER", nullable: false),
                    RedemptionDate = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointRedemptions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointRedemptions");
        }
    }
}
