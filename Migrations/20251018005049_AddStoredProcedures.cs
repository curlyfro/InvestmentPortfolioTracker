using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentPortfolioTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // sp_GetAllHoldings
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GetAllHoldings
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT 
                        Id, Symbol, AssetName, AssetType, Quantity, 
                        PurchasePrice, PurchaseDate, CurrentPrice, 
                        LastPriceUpdate, CreatedAt
                    FROM Holdings
                    ORDER BY Symbol;
                END
            ");

            // sp_GetHoldingById
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GetHoldingById
                    @Id INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT 
                        Id, Symbol, AssetName, AssetType, Quantity, 
                        PurchasePrice, PurchaseDate, CurrentPrice, 
                        LastPriceUpdate, CreatedAt
                    FROM Holdings
                    WHERE Id = @Id;
                END
            ");

            // sp_InsertHolding
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_InsertHolding
                    @Symbol NVARCHAR(10),
                    @AssetName NVARCHAR(100),
                    @AssetType NVARCHAR(20),
                    @Quantity DECIMAL(18,8),
                    @PurchasePrice DECIMAL(18,2),
                    @PurchaseDate DATETIME2,
                    @CurrentPrice DECIMAL(18,2) = NULL,
                    @LastPriceUpdate DATETIME2 = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    INSERT INTO Holdings (
                        Symbol, AssetName, AssetType, Quantity, 
                        PurchasePrice, PurchaseDate, CurrentPrice, 
                        LastPriceUpdate, CreatedAt
                    )
                    VALUES (
                        @Symbol, @AssetName, @AssetType, @Quantity,
                        @PurchasePrice, @PurchaseDate, @CurrentPrice,
                        @LastPriceUpdate, GETUTCDATE()
                    );
                    
                    SELECT 
                        Id, Symbol, AssetName, AssetType, Quantity, 
                        PurchasePrice, PurchaseDate, CurrentPrice, 
                        LastPriceUpdate, CreatedAt
                    FROM Holdings
                    WHERE Id = SCOPE_IDENTITY();
                END
            ");

            // sp_UpdateHolding
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_UpdateHolding
                    @Id INT,
                    @Symbol NVARCHAR(10),
                    @AssetName NVARCHAR(100),
                    @AssetType NVARCHAR(20),
                    @Quantity DECIMAL(18,8),
                    @PurchasePrice DECIMAL(18,2),
                    @PurchaseDate DATETIME2,
                    @CurrentPrice DECIMAL(18,2) = NULL,
                    @LastPriceUpdate DATETIME2 = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    UPDATE Holdings
                    SET 
                        Symbol = @Symbol,
                        AssetName = @AssetName,
                        AssetType = @AssetType,
                        Quantity = @Quantity,
                        PurchasePrice = @PurchasePrice,
                        PurchaseDate = @PurchaseDate,
                        CurrentPrice = @CurrentPrice,
                        LastPriceUpdate = @LastPriceUpdate
                    WHERE Id = @Id;
                    
                    SELECT 
                        Id, Symbol, AssetName, AssetType, Quantity, 
                        PurchasePrice, PurchaseDate, CurrentPrice, 
                        LastPriceUpdate, CreatedAt
                    FROM Holdings
                    WHERE Id = @Id;
                END
            ");

            // sp_UpdateHoldingPrice
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_UpdateHoldingPrice
                    @Id INT,
                    @CurrentPrice DECIMAL(18,2)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    UPDATE Holdings
                    SET 
                        CurrentPrice = @CurrentPrice,
                        LastPriceUpdate = GETUTCDATE()
                    WHERE Id = @Id;
                    
                    SELECT 
                        Id, Symbol, AssetName, AssetType, Quantity, 
                        PurchasePrice, PurchaseDate, CurrentPrice, 
                        LastPriceUpdate, CreatedAt
                    FROM Holdings
                    WHERE Id = @Id;
                END
            ");

            // sp_DeleteHolding
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_DeleteHolding
                    @Id INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    DELETE FROM Holdings
                    WHERE Id = @Id;
                    
                    SELECT @@ROWCOUNT AS RowsAffected;
                END
            ");

            // sp_HoldingExists
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_HoldingExists
                    @Id INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    SELECT CAST(COUNT(1) AS BIT) AS [Exists]
                    FROM Holdings
                    WHERE Id = @Id;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_HoldingExists");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_DeleteHolding");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_UpdateHoldingPrice");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_UpdateHolding");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_InsertHolding");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetHoldingById");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetAllHoldings");
        }
    }
}