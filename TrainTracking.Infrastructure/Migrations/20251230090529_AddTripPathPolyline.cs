using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripPathPolyline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PathPolyline",
                table: "Trips",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathPolyline",
                table: "Trips");
        }
    }
}
