using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class RecipeStepCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeSteps_Commands_CommandId",
                table: "RecipeSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeSteps_Recipes_SubrecipeId",
                table: "RecipeSteps");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeSteps_Commands_CommandId",
                table: "RecipeSteps",
                column: "CommandId",
                principalTable: "Commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeSteps_Recipes_SubrecipeId",
                table: "RecipeSteps",
                column: "SubrecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeSteps_Commands_CommandId",
                table: "RecipeSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeSteps_Recipes_SubrecipeId",
                table: "RecipeSteps");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeSteps_Commands_CommandId",
                table: "RecipeSteps",
                column: "CommandId",
                principalTable: "Commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeSteps_Recipes_SubrecipeId",
                table: "RecipeSteps",
                column: "SubrecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
