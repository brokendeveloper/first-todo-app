using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyTodo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEntityAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Todos",
                table: "Todos");

            migrationBuilder.RenameTable(
                name: "Todos",
                newName: "todos");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "todos",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Done",
                table: "todos",
                newName: "done");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "todos",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "todos",
                newName: "id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "todos",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "todos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "audit_logs",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_todos",
                table: "todos",
                column: "id");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_todos_user_id",
                table: "todos",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_todos_users_user_id",
                table: "todos",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_todos_users_user_id",
                table: "todos");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_todos",
                table: "todos");

            migrationBuilder.DropIndex(
                name: "IX_todos_user_id",
                table: "todos");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "todos");

            migrationBuilder.RenameTable(
                name: "todos",
                newName: "Todos");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Todos",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "done",
                table: "Todos",
                newName: "Done");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "Todos",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Todos",
                newName: "Id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Todos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "audit_logs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Todos",
                table: "Todos",
                column: "Id");
        }
    }
}
