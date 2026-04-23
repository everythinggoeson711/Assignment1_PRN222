using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicTrackingCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackingCode",
                table: "ReferralRequests",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrackingCode",
                table: "CenterServiceBookings",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRequests_TrackingCode",
                table: "ReferralRequests",
                column: "TrackingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CenterServiceBookings_TrackingCode",
                table: "CenterServiceBookings",
                column: "TrackingCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReferralRequests_TrackingCode",
                table: "ReferralRequests");

            migrationBuilder.DropIndex(
                name: "IX_CenterServiceBookings_TrackingCode",
                table: "CenterServiceBookings");

            migrationBuilder.DropColumn(
                name: "TrackingCode",
                table: "ReferralRequests");

            migrationBuilder.DropColumn(
                name: "TrackingCode",
                table: "CenterServiceBookings");
        }
    }
}
