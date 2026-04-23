using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCenterServiceBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CenterServiceBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ParticipantEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ParticipantPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    TherapistId = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeSlot = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    BookingStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PendingPayment"),
                    StripeSessionId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CenterServiceBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CenterServiceBookings_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CenterServiceBookings_Therapists_TherapistId",
                        column: x => x.TherapistId,
                        principalTable: "Therapists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CenterServiceBookings_TherapyServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "TherapyServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CenterServiceBookings_AppointmentId",
                table: "CenterServiceBookings",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CenterServiceBookings_ServiceId",
                table: "CenterServiceBookings",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CenterServiceBookings_TherapistId",
                table: "CenterServiceBookings",
                column: "TherapistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CenterServiceBookings");
        }
    }
}
