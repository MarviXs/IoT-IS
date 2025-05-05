using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class ProductDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BulbPlantingDistanceCm",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BulbPlantingPeriod",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BulbPlantingRequirementSqM",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CultivationTemperatureC",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CultivationTimeForBulbsWeeks",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CultivationTimeFromYoungPlant",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CultivationTimeSowingToPlant",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CultivationTimeVegetableWeek",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FlowersInFirstYear",
                table: "Products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GerminationTemperatureC",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GerminationTimeDays",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GrowthInhibitorsUsed",
                table: "Products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeightCm",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NaturalFloweringMonth",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumberOfBulbsPerPot",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlantSpacingCm",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlantingDensity",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PotSizeCm",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedSpacingCM",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedsMioHa",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedsPerThousandPlants",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedsPerThousandPots",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SowingPeriod",
                table: "Products",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BulbPlantingDistanceCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BulbPlantingPeriod",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BulbPlantingRequirementSqM",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CultivationTemperatureC",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CultivationTimeForBulbsWeeks",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CultivationTimeFromYoungPlant",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CultivationTimeSowingToPlant",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CultivationTimeVegetableWeek",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FlowersInFirstYear",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GerminationTemperatureC",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GerminationTimeDays",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GrowthInhibitorsUsed",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NaturalFloweringMonth",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NumberOfBulbsPerPot",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PlantSpacingCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PlantingDensity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PotSizeCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeedSpacingCM",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeedsMioHa",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeedsPerThousandPlants",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeedsPerThousandPots",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SowingPeriod",
                table: "Products");
        }
    }
}
