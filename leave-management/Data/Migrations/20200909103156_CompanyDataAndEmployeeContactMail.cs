using Microsoft.EntityFrameworkCore.Migrations;

namespace LeaveManagement.Data.Migrations
{
    public partial class CompanyDataAndEmployeeContactMail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyData",
                table: "Companies");

            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyPostAddress",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyProtectedComment",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyPublicComment",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyState",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyZipCode",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactMail",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyPostAddress",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyProtectedComment",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyPublicComment",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyState",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyZipCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContactMail",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "CompanyData",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
