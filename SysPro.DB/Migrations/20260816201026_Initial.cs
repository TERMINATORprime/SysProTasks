using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SysPro.DB.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "App");

            migrationBuilder.EnsureSchema(
                name: "Orders");

            migrationBuilder.CreateTable(
                name: "ImportAudits",
                schema: "App",
                columns: table => new
                {
                    ImportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Considered = table.Column<int>(type: "int", nullable: false),
                    Applied = table.Column<int>(type: "int", nullable: false),
                    Invalid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportAudits", x => x.ImportId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    OrderExternalID = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "OrderLines",
                schema: "Orders",
                columns: table => new
                {
                    OrderLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    CustomerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPriceCents = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    OrderID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLines", x => x.OrderLineId);
                    table.ForeignKey(
                        name: "FK_OrderLines_Orders_OrderID",
                        column: x => x.OrderID,
                        principalSchema: "Orders",
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderVersion",
                schema: "Orders",
                columns: table => new
                {
                    OrderVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    VersionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderVersion", x => x.OrderVersionId);
                    table.ForeignKey(
                        name: "FK_OrderVersion_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Orders",
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_OrderID",
                schema: "Orders",
                table: "OrderLines",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderExternalID",
                schema: "Orders",
                table: "Orders",
                column: "OrderExternalID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderVersion_OrderId",
                schema: "Orders",
                table: "OrderVersion",
                column: "OrderId");
            
            var asm = typeof(Initial).Assembly;
            
            RunScript(migrationBuilder, asm, "TVPTables.sql");
            foreach (var res in asm.GetManifestResourceNames()
                         .Where(n => n.EndsWith(".sql") && !n.EndsWith("TVPTables.sql")))
                RunScript(migrationBuilder, asm, res);  
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportAudits",
                schema: "App");

            migrationBuilder.DropTable(
                name: "OrderLines",
                schema: "Orders");

            migrationBuilder.DropTable(
                name: "OrderVersion",
                schema: "Orders");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "Orders");
        }
        
        static void RunScript(MigrationBuilder mb, Assembly asm, string endsWith)
        {
            var name = asm.GetManifestResourceNames().Single(n => n.EndsWith(endsWith));
            using var sr = new StreamReader(asm.GetManifestResourceStream(name)!);
            var text = sr.ReadToEnd();
            foreach (var batch in Regex.Split(text, @"^\s*GO\s*$",
                         RegexOptions.Multiline | RegexOptions.IgnoreCase))
                if (!string.IsNullOrWhiteSpace(batch))
                    mb.Sql(batch);
        }
    }
}
