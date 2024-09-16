using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoryQuizApp.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer",
                table: "Questions");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Quizzes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswers",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "CorrectAnswers",
                table: "Questions");

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
