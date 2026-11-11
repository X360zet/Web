using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDesk.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_ApprovedBy",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                table: "Transactions",
                newName: "AssigneeId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_ApprovedBy",
                table: "Transactions",
                newName: "IX_Transactions_AssigneeId");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Transactions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_AssigneeId",
                table: "Transactions",
                column: "AssigneeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_AssigneeId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "AssigneeId",
                table: "Transactions",
                newName: "ApprovedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_AssigneeId",
                table: "Transactions",
                newName: "IX_Transactions_ApprovedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_ApprovedBy",
                table: "Transactions",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
