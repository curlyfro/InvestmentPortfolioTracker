# Investment Portfolio Tracker - MVP

A C# .NET 8 console application for tracking investment holdings with manual price management.

## Features

### Implemented (MVP)
- ✅ Add new holdings (stocks, ETFs, crypto, bonds)
- ✅ View all holdings with detailed information
- ✅ Manual price updates (individual or batch)
- ✅ Delete holdings with confirmation
- ✅ Portfolio summary with performance metrics
- ✅ SQL Server database storage (AWS RDS)
- ✅ Beautiful console UI using Spectre.Console
- ✅ Automatic gain/loss calculations
- ✅ Best/worst performer tracking

## Prerequisites

- .NET 8 SDK or later
- SQL Server database (AWS RDS instance configured)
- Internet connection for database access

## Database Configuration

The application is pre-configured to connect to an AWS RDS SQL Server instance. The connection string is stored in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=[your database endpoing];Database=investment-tracker;User Id=admin;Password=***;TrustServerCertificate=True;Encrypt=True;"
  }
}
```

⚠️ **Security Note**: The connection string includes credentials. In production, use environment variables or Azure Key Vault.

## Installation & Setup

1. **Clone the repository** (or navigate to the project directory)
   ```bash
   cd InvestmentPortfolioTracker
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the application**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

The application will automatically:
- Connect to the database
- Create the database schema if it doesn't exist
- Display the main menu

## Usage

### Main Menu Options

```
=================================
INVESTMENT PORTFOLIO TRACKER
=================================
1. View Portfolio Summary
2. Add New Holding
3. Update Prices
4. View Holdings Details
5. Delete Holding
0. Exit
```

### Adding a New Holding

1. Select option **2** from the main menu
2. Enter the following information:
   - **Symbol/Ticker**: e.g., AAPL, BTC, VTSAX
   - **Asset Name**: Full name of the asset
   - **Asset Type**: Stock, ETF, Crypto, or Bond
   - **Quantity**: Number of shares/units (supports decimals)
   - **Purchase Price**: Price per unit at time of purchase
   - **Purchase Date**: Date of purchase (yyyy-MM-dd format)
   - **Current Price** (optional): Current market price

### Viewing Portfolio Summary

The summary displays:
- Total invested amount
- Current portfolio value
- Overall gain/loss ($ and %)
- Number of holdings
- Top 5 holdings by value
- Best and worst performers

### Updating Prices

Two options available:
- **Update a specific holding**: Select one holding to update
- **Update all holdings**: Update prices for all holdings at once

### Viewing Holdings Details

Displays a comprehensive table with:
- ID, Symbol, Asset Name, Type
- Quantity, Purchase Price, Purchase Date
- Current Price, Current Value
- Gain/Loss ($), Gain/Loss (%)

### Deleting a Holding

1. Select option **5**
2. Choose the holding to delete
3. Confirm the deletion

## Project Structure

```
InvestmentPortfolioTracker/
├── Models/
│   └── Holding.cs              # Entity model with calculated properties
├── Data/
│   ├── PortfolioDbContext.cs   # EF Core database context
│   ├── PortfolioDbContextFactory.cs  # Design-time factory for migrations
│   └── Migrations/             # EF Core migrations
├── Repositories/
│   ├── IHoldingRepository.cs   # Repository interface
│   └── HoldingRepository.cs    # Repository implementation
├── Services/
│   └── PortfolioService.cs     # Business logic layer
├── UI/
│   ├── MenuHandler.cs          # Menu navigation and user interaction
│   └── DisplayFormatters.cs    # Console output formatting
├── Program.cs                  # Application entry point
├── appsettings.json           # Configuration file
└── InvestmentPortfolioTracker.csproj
```

## Database Schema

### Holdings Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-generated) |
| Symbol | nvarchar(10) | Stock ticker or symbol |
| AssetName | nvarchar(100) | Full name of the asset |
| AssetType | nvarchar(20) | Stock, ETF, Crypto, or Bond |
| Quantity | decimal(18,8) | Number of units held |
| PurchasePrice | decimal(18,2) | Price per unit at purchase |
| PurchaseDate | datetime2 | Date of purchase |
| CurrentPrice | decimal(18,2) | Current market price (nullable) |
| LastPriceUpdate | datetime2 | Last time price was updated (nullable) |
| CreatedAt | datetime2 | Record creation timestamp |
### Stored Procedures

The application uses stored procedures for all database operations, providing better performance and security. The following stored procedures are available:

| Stored Procedure | Purpose | Parameters |
|-----------------|---------|------------|
| `sp_GetAllHoldings` | Retrieve all holdings ordered by symbol | None |
| `sp_GetHoldingById` | Get a specific holding | @Id (int) |
| `sp_InsertHolding` | Create a new holding | @Symbol, @AssetName, @AssetType, @Quantity, @PurchasePrice, @PurchaseDate, @CurrentPrice, @LastPriceUpdate |
| `sp_UpdateHolding` | Update all fields of a holding | @Id, @Symbol, @AssetName, @AssetType, @Quantity, @PurchasePrice, @PurchaseDate, @CurrentPrice, @LastPriceUpdate |
| `sp_UpdateHoldingPrice` | Optimized price-only update | @Id, @CurrentPrice |
| `sp_DeleteHolding` | Remove a holding | @Id |
| `sp_HoldingExists` | Check if holding exists | @Id |

**Benefits of using stored procedures:**
- ✅ **Performance**: Compiled execution plans for faster queries
- ✅ **Security**: Parameterized queries prevent SQL injection
- ✅ **Maintainability**: Database logic separated from application code
- ✅ **Optimization**: Database team can tune procedures without code changes


## Example Usage

### Adding a Stock
```
Symbol: AAPL
Asset Name: Apple Inc.
Asset Type: Stock
Quantity: 50
Purchase Price: 150.00
Purchase Date: 2024-01-15
Current Price: 175.50
```

### Portfolio Summary Output
```
Portfolio Value: $45,230.50
Total Invested: $38,500.00
Total Gain/Loss: $6,730.50 (+17.48%)
Holdings: 5 total, 5 with prices

Top Holdings by Value:
AAPL  $8,775.00  +$1,275.00  +16.99%
BTC   $21,500.00 +$3,000.00  +16.22%
VTSAX $12,300.00 +$800.00    +6.95%

Best Performer: BTC (+18.52%)
Worst Performer: BOND1 (-2.15%)
```

## Technologies Used

- **.NET 8.0**: Latest .NET framework
- **Entity Framework Core 9.0**: ORM for database operations
- **SQL Server**: Database (AWS RDS)
- **Spectre.Console**: Rich console UI library
- **C# 12**: Modern C# features

## Future Enhancements (Not in MVP)

- 🔄 Automatic price updates via API (Alpha Vantage, Yahoo Finance, CoinGecko)
- 📊 Transaction history tracking
- 💰 Realized vs unrealized gains
- 📈 Performance reports over time
- 🥧 Portfolio allocation by asset type
- 📤 Export to CSV/Excel
- 🔐 User authentication
- 📱 Multiple portfolio support
- 🏦 Tax lot tracking (FIFO/LIFO)

## Troubleshooting

### Database Connection Issues

If you see connection errors:
1. Verify the AWS RDS instance is running
2. Check security group allows your IP
3. Verify credentials in appsettings.json
4. Ensure SQL Server port (1433) is accessible

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Missing EF Tools

```bash
# Install EF Core CLI tools globally
dotnet tool install --global dotnet-ef
```

## Development

### Creating New Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Running in Development

```bash
dotnet run
```

### Building for Release

```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

## License

This is a portfolio project for educational purposes.

## Author

Created as an MVP for investment portfolio tracking with plans for future enhancements.