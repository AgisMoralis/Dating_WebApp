using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingApp.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class MapPhotosToMembersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Photos",
                newName: "MemberId"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Photos",
                newName: "UserId"
                );
        }
    }
}
