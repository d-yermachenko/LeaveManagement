using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LeaveManagement.Data.Migrations
{
    public partial class EmployeeLastLoginDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentConnectionDate",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: DateTime.MinValue);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastConnectionDate",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: DateTime.MinValue) ;
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentConnectionDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastConnectionDate",
                table: "AspNetUsers");
        }
    }
}
