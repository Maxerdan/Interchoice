using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Interchoice.Migrations
{
    public partial class InitialCreate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProjectInfo",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "FullDescription",
                table: "ProjectInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProjectInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "ProjectInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "ProjectInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Node",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentGuids = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChildGuids = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Node", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Node");

            migrationBuilder.DropColumn(
                name: "FullDescription",
                table: "ProjectInfo");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProjectInfo");

            migrationBuilder.DropColumn(
                name: "Overview",
                table: "ProjectInfo");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "ProjectInfo");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ProjectInfo",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
