using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_mvc.Migrations
{
    /// <inheritdoc />
    public partial class add_end_date : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Sessions",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Sessions");
        }
    }
}
