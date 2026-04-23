using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferralRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NdisNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParticipantPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParticipantEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HomeAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ReferrerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Organisation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReferrerEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReferrerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReferrerRole = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    PreferredTherapistId = table.Column<int>(type: "int", nullable: true),
                    PrimaryReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MainReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    KeyConcerns = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DesiredOutcomes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UrgencyLevel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PreferredContactMethod = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PrivacyConsent = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "New"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralRequests_Therapists_PreferredTherapistId",
                        column: x => x.PreferredTherapistId,
                        principalTable: "Therapists",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReferralRequests_TherapyServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "TherapyServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRequests_PreferredTherapistId",
                table: "ReferralRequests",
                column: "PreferredTherapistId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRequests_ServiceId",
                table: "ReferralRequests",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferralRequests");
        }
    }
}
