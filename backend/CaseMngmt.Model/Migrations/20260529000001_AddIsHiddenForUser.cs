using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseMngmt.Models.Migrations
{
    public partial class AddIsHiddenForUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHiddenForUser",
                table: "Keyword",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHiddenForUser",
                table: "Keyword");
        }
    }
}
