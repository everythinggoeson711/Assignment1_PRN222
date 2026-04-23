using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Montra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TherapyServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapyServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Therapists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Therapists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Therapists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PatientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PatientEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TherapistId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeSlot = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TherapyNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Therapists_TherapistId",
                        column: x => x.TherapistId,
                        principalTable: "Therapists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_TherapyServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "TherapyServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TherapyServices",
                columns: new[] { "Id", "Description", "DurationMinutes", "IsActive", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Phiên tư vấn 1-1 với chuyên gia.", 60, true, "Tư vấn cá nhân", 500000m },
                    { 2, "Trị liệu theo nhóm nhỏ 4-6 người.", 90, true, "Trị liệu nhóm", 250000m },
                    { 3, "Tư vấn các vấn đề hôn nhân và quan hệ gia đình.", 90, true, "Tư vấn hôn nhân & gia đình", 700000m }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@montra.com", "Quản Trị Viên", "e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7", "Admin" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "khoa@montra.com", "Nguyễn Minh Khoa", "619be98e6787d4b0ccab7e7403010f29b7b56bb2afb04567954a57f1b7797e51", "Therapist" },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "lan@montra.com", "Trần Thị Lan", "619be98e6787d4b0ccab7e7403010f29b7b56bb2afb04567954a57f1b7797e51", "Therapist" }
                });

            migrationBuilder.InsertData(
                table: "Therapists",
                columns: new[] { "Id", "Bio", "IsActive", "Name", "Phone", "Specialty", "UserId" },
                values: new object[,]
                {
                    { 1, "Chuyên gia tâm lý hơn 8 năm kinh nghiệm điều trị trầm cảm và rối loạn tâm trạng.", true, "Nguyễn Minh Khoa", "0901234567", "Trầm cảm", 2 },
                    { 2, "Tiến sĩ tâm lý lâm sàng, chuyên điều trị lo âu và stress mạn tính.", true, "Trần Thị Lan", "0907654321", "Rối loạn lo âu", 3 }
                });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "AppointmentDate", "CreatedAt", "PatientEmail", "PatientName", "PatientPhone", "ServiceId", "Status", "TherapistId", "TherapyNotes", "TimeSlot" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "an@email.com", "Lê Văn An", "0912000001", 1, 1, 1, null, "09:00 - 10:00" },
                    { 2, new DateTime(2024, 6, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "binh@email.com", "Phạm Thị Bình", "0912000002", 2, 0, 2, null, "10:00 - 11:30" },
                    { 3, new DateTime(2024, 6, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "cuong@email.com", "Hoàng Văn Cường", "0912000003", 3, 2, 1, "Bệnh nhân tiến triển tốt, giảm lo âu rõ rệt.", "14:00 - 15:30" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TherapistId",
                table: "Appointments",
                column: "TherapistId");

            migrationBuilder.CreateIndex(
                name: "IX_Therapists_UserId",
                table: "Therapists",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Therapists");

            migrationBuilder.DropTable(
                name: "TherapyServices");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
