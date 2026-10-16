using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketsAndCommissionPerUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Markets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Markets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Added as nullable first so existing trade history can be attached
            // to a market before the foreign key becomes mandatory.
            migrationBuilder.AddColumn<int>(
                name: "MarketId",
                table: "Trades",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPerUnit",
                table: "Trades",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_MarketId",
                table: "Trades",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Markets_UserId_Name",
                table: "Markets",
                columns: new[] { "UserId", "Name" },
                unique: true);

            // Preserve existing trade history: give every user that already has
            // trades a default market and attach their trades to it, so no rows
            // are lost when MarketId becomes required.
            migrationBuilder.Sql(
                """
                INSERT INTO "Markets" ("UserId", "Name", "Currency", "CreatedAt", "UpdatedAt")
                SELECT DISTINCT t."UserId", 'Imported', 'USD', datetime('now'), datetime('now')
                FROM "Trades" t
                WHERE NOT EXISTS (
                    SELECT 1 FROM "Markets" m WHERE m."UserId" = t."UserId");

                UPDATE "Trades"
                SET "MarketId" = (
                    SELECT m."Id" FROM "Markets" m
                    WHERE m."UserId" = "Trades"."UserId"
                    ORDER BY m."Id"
                    LIMIT 1)
                WHERE "MarketId" IS NULL;

                -- Backfill the per-unit rate from the stored total so
                -- Commission = CommissionPerUnit x Quantity keeps holding.
                UPDATE "Trades"
                SET "CommissionPerUnit" = ROUND(CAST("Commission" AS REAL) / "Quantity", 4)
                WHERE "Quantity" > 0;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "MarketId",
                table: "Trades",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_Markets_MarketId",
                table: "Trades",
                column: "MarketId",
                principalTable: "Markets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trades_Markets_MarketId",
                table: "Trades");

            migrationBuilder.DropTable(
                name: "Markets");

            migrationBuilder.DropIndex(
                name: "IX_Trades_MarketId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "CommissionPerUnit",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "MarketId",
                table: "Trades");
        }
    }
}
