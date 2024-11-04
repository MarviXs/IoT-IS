using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class IS_Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Title2 = table.Column<string>(type: "text", nullable: false),
                    Ic = table.Column<string>(type: "text", nullable: false),
                    Dic = table.Column<string>(type: "text", nullable: false),
                    Ulice = table.Column<string>(type: "text", nullable: false),
                    Psc = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    PLUCode = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    LatinName = table.Column<string>(type: "text", nullable: false),
                    CzechName = table.Column<string>(type: "text", nullable: false),
                    FlowerLeafDescription = table.Column<string>(type: "text", nullable: false),
                    PotDiameterPack = table.Column<string>(type: "text", nullable: false),
                    PricePerPiecePack = table.Column<decimal>(type: "numeric", nullable: false),
                    PricePerPiecePackVAT = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountedPriceWithoutVAT = table.Column<decimal>(type: "numeric", nullable: false),
                    RetailPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.PLUCode);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DeliveryNotes",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryNumber = table.Column<string>(type: "text", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    VatGroup15 = table.Column<decimal>(type: "numeric", nullable: false),
                    VatGroup21 = table.Column<decimal>(type: "numeric", nullable: false),
                    VatTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmountWithVat = table.Column<decimal>(type: "numeric", nullable: false),
                    Took = table.Column<string>(type: "text", nullable: false),
                    Forwarded = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    LicensePlate = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryNotes_Companies_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DeliveryNotes_Companies_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Companies_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Invoices_Companies_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Note = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveryWeek = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    ContactPhone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Companies_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "WorkReports",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportNumber = table.Column<string>(type: "text", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkReports_Companies_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_WorkReports_Companies_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AdditionalOrders",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<string>(type: "text", nullable: false),
                    Company = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalOrders_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "PLUCode",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ProductionPlans",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    ProductNumber = table.Column<string>(type: "text", nullable: false),
                    DeliveryWeek = table.Column<int>(type: "integer", nullable: false),
                    OrderedQuantity = table.Column<int>(type: "integer", nullable: false),
                    StrQuantity = table.Column<int>(type: "integer", nullable: false),
                    CbQuantity = table.Column<int>(type: "integer", nullable: false),
                    VolyneQuantity = table.Column<int>(type: "integer", nullable: false),
                    TabQuantity = table.Column<int>(type: "integer", nullable: false),
                    StoreQuantity = table.Column<int>(type: "integer", nullable: false),
                    Pot9 = table.Column<int>(type: "integer", nullable: false),
                    Pot10 = table.Column<int>(type: "integer", nullable: false),
                    Pot12 = table.Column<int>(type: "integer", nullable: false),
                    Pot14 = table.Column<int>(type: "integer", nullable: false),
                    Pot17 = table.Column<int>(type: "integer", nullable: false),
                    Pot21 = table.Column<int>(type: "integer", nullable: false),
                    Pack10 = table.Column<int>(type: "integer", nullable: false),
                    Pack6 = table.Column<int>(type: "integer", nullable: false),
                    Z = table.Column<int>(type: "integer", nullable: false),
                    M6 = table.Column<int>(type: "integer", nullable: false),
                    M10 = table.Column<int>(type: "integer", nullable: false),
                    S84 = table.Column<int>(type: "integer", nullable: false),
                    TotalQuantity = table.Column<int>(type: "integer", nullable: false),
                    ActualQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionPlans_Products_ProductNumber",
                        column: x => x.ProductNumber,
                        principalTable: "Products",
                        principalColumn: "PLUCode",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Summaries",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Place = table.Column<string>(type: "text", nullable: false),
                    ProductNumber = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Summaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Summaries_Products_ProductNumber",
                        column: x => x.ProductNumber,
                        principalTable: "Products",
                        principalColumn: "PLUCode",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DeliveryItems",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryNoteId = table.Column<int>(type: "integer", nullable: false),
                    PluCode = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    PlantPassport = table.Column<string>(type: "text", nullable: false),
                    PackSize = table.Column<string>(type: "text", nullable: false),
                    UnitPriceWithoutVat = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPriceWithoutVat = table.Column<decimal>(type: "numeric", nullable: false),
                    VatRate = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_DeliveryNotes_DeliveryNoteId",
                        column: x => x.DeliveryNoteId,
                        principalTable: "DeliveryNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    PluCode = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ItemDescription = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ProductNumber = table.Column<string>(type: "text", nullable: false),
                    VarietyName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductNumber",
                        column: x => x.ProductNumber,
                        principalTable: "Products",
                        principalColumn: "PLUCode",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "WorkDayDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ReportId = table.Column<int>(type: "integer", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TaskNumber = table.Column<int>(type: "integer", nullable: false),
                    WorkLocation = table.Column<string>(type: "text", nullable: false),
                    WorkType = table.Column<string>(type: "text", nullable: false),
                    WorkersCount = table.Column<int>(type: "integer", nullable: false),
                    WorkHours = table.Column<int>(type: "integer", nullable: false),
                    RateA = table.Column<decimal>(type: "numeric", nullable: false),
                    RateB = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalA = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalB = table.Column<decimal>(type: "numeric", nullable: false),
                    Equipment = table.Column<string>(type: "text", nullable: false),
                    TotalEquipment = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkDayDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkDayDetails_WorkReports_Id",
                        column: x => x.Id,
                        principalTable: "WorkReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(name: "IX_AdditionalOrders_ProductId", table: "AdditionalOrders", column: "ProductId");

            migrationBuilder.CreateIndex(name: "IX_DeliveryItems_DeliveryNoteId", table: "DeliveryItems", column: "DeliveryNoteId");

            migrationBuilder.CreateIndex(name: "IX_DeliveryNotes_CustomerId", table: "DeliveryNotes", column: "CustomerId");

            migrationBuilder.CreateIndex(name: "IX_DeliveryNotes_SupplierId", table: "DeliveryNotes", column: "SupplierId");

            migrationBuilder.CreateIndex(name: "IX_InvoiceItems_InvoiceId", table: "InvoiceItems", column: "InvoiceId");

            migrationBuilder.CreateIndex(name: "IX_Invoices_CustomerId", table: "Invoices", column: "CustomerId");

            migrationBuilder.CreateIndex(name: "IX_Invoices_SupplierId", table: "Invoices", column: "SupplierId");

            migrationBuilder.CreateIndex(name: "IX_OrderItems_OrderId", table: "OrderItems", column: "OrderId");

            migrationBuilder.CreateIndex(name: "IX_OrderItems_ProductNumber", table: "OrderItems", column: "ProductNumber");

            migrationBuilder.CreateIndex(name: "IX_Orders_CustomerId", table: "Orders", column: "CustomerId");

            migrationBuilder.CreateIndex(name: "IX_ProductionPlans_ProductNumber", table: "ProductionPlans", column: "ProductNumber");

            migrationBuilder.CreateIndex(name: "IX_Products_CategoryId", table: "Products", column: "CategoryId");

            migrationBuilder.CreateIndex(name: "IX_Summaries_ProductNumber", table: "Summaries", column: "ProductNumber");

            migrationBuilder.CreateIndex(name: "IX_WorkReports_CustomerId", table: "WorkReports", column: "CustomerId");

            migrationBuilder.CreateIndex(name: "IX_WorkReports_SupplierId", table: "WorkReports", column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AdditionalOrders");

            migrationBuilder.DropTable(name: "DeliveryItems");

            migrationBuilder.DropTable(name: "InvoiceItems");

            migrationBuilder.DropTable(name: "OrderItems");

            migrationBuilder.DropTable(name: "ProductionPlans");

            migrationBuilder.DropTable(name: "Summaries");

            migrationBuilder.DropTable(name: "WorkDayDetails");

            migrationBuilder.DropTable(name: "DeliveryNotes");

            migrationBuilder.DropTable(name: "Invoices");

            migrationBuilder.DropTable(name: "Orders");

            migrationBuilder.DropTable(name: "Products");

            migrationBuilder.DropTable(name: "WorkReports");

            migrationBuilder.DropTable(name: "Categories");

            migrationBuilder.DropTable(name: "Companies");
        }
    }
}
