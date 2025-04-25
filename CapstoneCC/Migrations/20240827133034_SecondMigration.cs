using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CapstoneCC.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2adb5c6c-300b-4392-9250-aedd29eca7b8", null, "reseller", "reseller" },
                    { "a6b0c20f-7c1f-4603-8d8c-c417e68d0f96", null, "staff", "staff" },
                    { "b6fa8cab-61a4-4344-a08d-cb78d4492d4d", null, "customer", "customer" },
                    { "f57192a7-889c-4f1a-a463-e33cc74dfb01", null, "admin", "admin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2adb5c6c-300b-4392-9250-aedd29eca7b8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a6b0c20f-7c1f-4603-8d8c-c417e68d0f96");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b6fa8cab-61a4-4344-a08d-cb78d4492d4d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f57192a7-889c-4f1a-a463-e33cc74dfb01");
        }
    }
}
