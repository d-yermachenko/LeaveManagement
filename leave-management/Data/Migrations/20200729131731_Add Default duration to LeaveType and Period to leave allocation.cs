using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LeaveManagement.Data.Migrations
{
    public partial class AddDefaultdurationtoLeaveTypeandPeriodtoleaveallocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "LeaveTypes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultDays",
                table: "LeaveTypes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "LeaveAllocations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                nullable: true);


            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_AuthorId",
                table: "LeaveTypes",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveTypes_AspNetUsers_AuthorId",
                table: "LeaveTypes",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveTypes_AspNetUsers_AuthorId",
                table: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_LeaveTypes_AuthorId",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "DefaultDays",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "LeaveAllocations");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");



        }
    }
}
