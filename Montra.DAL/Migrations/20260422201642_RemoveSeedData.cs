using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Montra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Therapists",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Therapists",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TherapyServices",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TherapyServices",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TherapyServices",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
