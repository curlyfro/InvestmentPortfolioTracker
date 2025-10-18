using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using InvestmentPortfolioTracker.Data;
using InvestmentPortfolioTracker.Repositories;
using InvestmentPortfolioTracker.Services;
using InvestmentPortfolioTracker.UI;
using Spectre.Console;

namespace InvestmentPortfolioTracker;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                AnsiConsole.MarkupLine("[red]Error: Connection string not found in appsettings.json[/]");
                return;
            }

            // Set up DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<PortfolioDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Initialize database context
            await using var context = new PortfolioDbContext(optionsBuilder.Options);

            // Check database connection and run migrations
            await AnsiConsole.Status()
                .StartAsync("Connecting to database and applying migrations...", async ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    try
                    {
                        // Apply pending migrations (creates database and tables if needed)
                        await context.Database.MigrateAsync();
                        AnsiConsole.MarkupLine("[green]✓[/] Connected to database successfully!");
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]✗ Database connection failed: {ex.Message}[/]");
                        throw;
                    }
                });

            // Set up dependency injection manually
            var repository = new HoldingRepository(context);
            var portfolioService = new PortfolioService(repository);
            var menuHandler = new MenuHandler(portfolioService);

            // Run the application
            await menuHandler.RunAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(ex,
                ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes |
                ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[red]Press any key to exit...[/]");
            Console.ReadKey();
        }
    }
}