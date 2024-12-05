using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SheridanBankingTeamProject.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Goals",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Goals",
                table: "Users");
        }
    }
}
