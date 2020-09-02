using Microsoft.EntityFrameworkCore.Migrations;

namespace LeaveManagement.Data.Migrations
{
    public partial class LeaveRequestsCommentsAndCancel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequestCancelled",
                table: "LeaveRequests",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RequestComment",
                table: "LeaveRequests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationComment",
                table: "LeaveRequests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestCancelled",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "RequestComment",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ValidationComment",
                table: "LeaveRequests");
        }
    }
}
