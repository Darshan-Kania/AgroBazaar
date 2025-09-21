using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgroBazaar.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeAuthPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2025, 9, 20, 8, 43, 8, 161, DateTimeKind.Utc).AddTicks(2291), "Fresh vegetables" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2025, 9, 20, 8, 43, 8, 161, DateTimeKind.Utc).AddTicks(2324), "Fresh fruits" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2025, 9, 20, 8, 43, 8, 161, DateTimeKind.Utc).AddTicks(2353), "Cereals and grains" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Description", "Name" },
                values: new object[] { new DateTime(2025, 9, 20, 8, 43, 8, 161, DateTimeKind.Utc).AddTicks(2382), "Dairy products", "Dairy" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedAt",
                table: "Products",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_CategoryId",
                table: "Products",
                columns: new[] { "IsActive", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_OrderDate",
                table: "Orders",
                columns: new[] { "CustomerId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive_UserType",
                table: "AspNetUsers",
                columns: new[] { "IsActive", "UserType" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserType",
                table: "AspNetUsers",
                column: "UserType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive_UserType",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserType",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2025, 9, 20, 5, 12, 17, 482, DateTimeKind.Utc).AddTicks(8901), "Fresh vegetables from local farms" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2025, 9, 20, 5, 12, 17, 482, DateTimeKind.Utc).AddTicks(8935), "Seasonal fresh fruits" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2025, 9, 20, 5, 12, 17, 482, DateTimeKind.Utc).AddTicks(8963), "Rice, wheat, and other grains" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Description", "Name" },
                values: new object[] { new DateTime(2025, 9, 20, 5, 12, 17, 482, DateTimeKind.Utc).AddTicks(8991), "Lentils, beans, and other pulses", "Pulses" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "IsActive", "Name" },
                values: new object[,]
                {
                    { 5, new DateTime(2025, 9, 20, 5, 12, 17, 482, DateTimeKind.Utc).AddTicks(9020), "Fresh milk and dairy products", null, true, "Dairy" },
                    { 6, new DateTime(2025, 9, 20, 5, 12, 17, 482, DateTimeKind.Utc).AddTicks(9049), "Fresh and dried spices", null, true, "Spices" }
                });
        }
    }
}
