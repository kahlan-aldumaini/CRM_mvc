using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_mvc.Migrations
{
    /// <inheritdoc />
    public partial class fix_chat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FromUser",
                table: "Chats",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromUser",
                table: "Chats");
        }
    }
}
