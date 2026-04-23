using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingPaymentExpiryAndBookingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CenterServiceBookings_TherapistId",
                table: "CenterServiceBookings");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TherapistId",
                table: "Appointments");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "CenterServiceBookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CenterServiceBookings_TherapistId_AppointmentDate_TimeSlot",
                table: "CenterServiceBookings",
                columns: new[] { "TherapistId", "AppointmentDate", "TimeSlot" },
                unique: true,
                filter: "[BookingStatus] = 'PendingPayment'");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TherapistId_AppointmentDate_TimeSlot",
                table: "Appointments",
                columns: new[] { "TherapistId", "AppointmentDate", "TimeSlot" },
                unique: true,
                filter: "[Status] <> 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CenterServiceBookings_TherapistId_AppointmentDate_TimeSlot",
                table: "CenterServiceBookings");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TherapistId_AppointmentDate_TimeSlot",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "CenterServiceBookings");

            migrationBuilder.CreateIndex(
                name: "IX_CenterServiceBookings_TherapistId",
                table: "CenterServiceBookings",
                column: "TherapistId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TherapistId",
                table: "Appointments",
                column: "TherapistId");
        }
    }
}
