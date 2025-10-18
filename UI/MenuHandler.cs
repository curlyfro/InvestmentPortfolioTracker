using Spectre.Console;
using InvestmentPortfolioTracker.Models;
using InvestmentPortfolioTracker.Services;

namespace InvestmentPortfolioTracker.UI;

public class MenuHandler
{
    private readonly PortfolioService _portfolioService;

    public MenuHandler(PortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            DisplayFormatters.ShowHeader();
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]What would you like to do?[/]")
                    .PageSize(10)
                    .AddChoices(new[]
                    {
                        "1. View Portfolio Summary",
                        "2. Add New Holding",
                        "3. Update Prices",
                        "4. View Holdings Details",
                        "5. Delete Holding",
                        "0. Exit"
                    })
            );

            try
            {
                switch (choice[0])
                {
                    case '1':
                        await ViewPortfolioSummaryAsync();
                        break;
                    case '2':
                        await AddNewHoldingAsync();
                        break;
                    case '3':
                        await UpdatePricesAsync();
                        break;
                    case '4':
                        await ViewHoldingsDetailsAsync();
                        break;
                    case '5':
                        await DeleteHoldingAsync();
                        break;
                    case '0':
                        AnsiConsole.MarkupLine("[bold yellow]Thank you for using Investment Portfolio Tracker![/]");
                        return;
                }
            }
            catch (Exception ex)
            {
                DisplayFormatters.ShowError(ex.Message);
                DisplayFormatters.PressAnyKey();
            }
        }
    }

    private async Task ViewPortfolioSummaryAsync()
    {
        DisplayFormatters.ShowHeader();
        AnsiConsole.Status()
            .Start("Loading portfolio data...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                Thread.Sleep(500); // Brief pause for visual feedback
            });

        var summary = await _portfolioService.GetPortfolioSummaryAsync();
        
        if (summary.HoldingsCount == 0)
        {
            DisplayFormatters.ShowWarning("No holdings found. Add some holdings to see your portfolio summary.");
        }
        else
        {
            DisplayFormatters.ShowPortfolioSummary(summary);
        }

        DisplayFormatters.PressAnyKey();
    }

    private async Task AddNewHoldingAsync()
    {
        DisplayFormatters.ShowHeader();
        AnsiConsole.MarkupLine("[bold underline]Add New Holding[/]\n");

        var symbol = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Enter symbol/ticker:[/]")
                .ValidationErrorMessage("[red]Symbol cannot be empty[/]")
                .Validate(s => !string.IsNullOrWhiteSpace(s))
        ).ToUpper();

        var assetName = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Enter asset name:[/]")
                .ValidationErrorMessage("[red]Asset name cannot be empty[/]")
                .Validate(s => !string.IsNullOrWhiteSpace(s))
        );

        var assetType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Select asset type:[/]")
                .AddChoices(new[] { "Stock", "ETF", "Crypto", "Bond" })
        );

        var quantity = AnsiConsole.Prompt(
            new TextPrompt<decimal>("[cyan]Enter quantity:[/]")
                .ValidationErrorMessage("[red]Quantity must be a positive number[/]")
                .Validate(q => q > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Quantity must be greater than zero[/]"))
        );

        var purchasePrice = AnsiConsole.Prompt(
            new TextPrompt<decimal>("[cyan]Enter purchase price:[/]")
                .ValidationErrorMessage("[red]Price must be a positive number[/]")
                .Validate(p => p > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Price must be greater than zero[/]"))
        );

        var purchaseDate = AnsiConsole.Prompt(
            new TextPrompt<DateTime>("[cyan]Enter purchase date (yyyy-MM-dd):[/]")
                .DefaultValue(DateTime.Today)
                .ValidationErrorMessage("[red]Invalid date format. Use yyyy-MM-dd[/]")
                .Validate(d => d <= DateTime.Now ? ValidationResult.Success() : ValidationResult.Error("[red]Date cannot be in the future[/]"))
        );

        var currentPrice = AnsiConsole.Confirm("[cyan]Do you want to enter the current price now?[/]", false)
            ? AnsiConsole.Prompt(
                new TextPrompt<decimal>("[cyan]Enter current price:[/]")
                    .ValidationErrorMessage("[red]Price must be a positive number[/]")
                    .Validate(p => p > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Price must be greater than zero[/]"))
            )
            : (decimal?)null;

        var holding = new Holding
        {
            Symbol = symbol,
            AssetName = assetName,
            AssetType = assetType,
            Quantity = quantity,
            PurchasePrice = purchasePrice,
            PurchaseDate = purchaseDate,
            CurrentPrice = currentPrice,
            LastPriceUpdate = currentPrice.HasValue ? DateTime.UtcNow : null
        };

        await _portfolioService.AddHoldingAsync(holding);
        DisplayFormatters.ShowSuccess($"Successfully added {symbol} to your portfolio!");
        DisplayFormatters.PressAnyKey();
    }

    private async Task UpdatePricesAsync()
    {
        DisplayFormatters.ShowHeader();
        AnsiConsole.MarkupLine("[bold underline]Update Prices[/]\n");

        var holdings = (await _portfolioService.GetAllHoldingsAsync()).ToList();
        
        if (!holdings.Any())
        {
            DisplayFormatters.ShowWarning("No holdings found. Add some holdings first.");
            DisplayFormatters.PressAnyKey();
            return;
        }

        var updateChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]How would you like to update prices?[/]")
                .AddChoices(new[]
                {
                    "Update a specific holding",
                    "Update all holdings",
                    "Cancel"
                })
        );

        if (updateChoice == "Cancel")
        {
            return;
        }

        if (updateChoice == "Update a specific holding")
        {
            var holdingChoices = holdings.Select(h => $"{h.Id}. {h.Symbol} - {h.AssetName}").ToList();
            holdingChoices.Add("Cancel");

            var selectedHolding = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Select a holding to update:[/]")
                    .PageSize(15)
                    .AddChoices(holdingChoices)
            );

            if (selectedHolding == "Cancel")
            {
                return;
            }

            var holdingId = int.Parse(selectedHolding.Split('.')[0]);
            var holding = holdings.First(h => h.Id == holdingId);

            var newPrice = AnsiConsole.Prompt(
                new TextPrompt<decimal>($"[cyan]Enter new price for {holding.Symbol}:[/]")
                    .DefaultValue(holding.CurrentPrice ?? holding.PurchasePrice)
                    .ValidationErrorMessage("[red]Price must be a positive number[/]")
                    .Validate(p => p > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Price must be greater than zero[/]"))
            );

            await _portfolioService.UpdatePriceAsync(holdingId, newPrice);
            DisplayFormatters.ShowSuccess($"Successfully updated price for {holding.Symbol}!");
        }
        else // Update all holdings
        {
            foreach (var holding in holdings)
            {
                var newPrice = AnsiConsole.Prompt(
                    new TextPrompt<decimal>($"[cyan]Enter new price for {holding.Symbol}:[/]")
                        .DefaultValue(holding.CurrentPrice ?? holding.PurchasePrice)
                        .ValidationErrorMessage("[red]Price must be a positive number[/]")
                        .Validate(p => p > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Price must be greater than zero[/]"))
                );

                await _portfolioService.UpdatePriceAsync(holding.Id, newPrice);
                DisplayFormatters.ShowSuccess($"Updated {holding.Symbol}");
            }

            DisplayFormatters.ShowSuccess("All prices have been updated!");
        }

        DisplayFormatters.PressAnyKey();
    }

    private async Task ViewHoldingsDetailsAsync()
    {
        DisplayFormatters.ShowHeader();
        AnsiConsole.MarkupLine("[bold underline]Holdings Details[/]\n");

        var holdings = (await _portfolioService.GetAllHoldingsAsync()).ToList();
        
        if (!holdings.Any())
        {
            DisplayFormatters.ShowWarning("No holdings found. Add some holdings to see details.");
        }
        else
        {
            DisplayFormatters.ShowHoldingsTable(holdings);
        }

        DisplayFormatters.PressAnyKey();
    }

    private async Task DeleteHoldingAsync()
    {
        DisplayFormatters.ShowHeader();
        AnsiConsole.MarkupLine("[bold underline]Delete Holding[/]\n");

        var holdings = (await _portfolioService.GetAllHoldingsAsync()).ToList();
        
        if (!holdings.Any())
        {
            DisplayFormatters.ShowWarning("No holdings found.");
            DisplayFormatters.PressAnyKey();
            return;
        }

        var holdingChoices = holdings.Select(h => $"{h.Id}. {h.Symbol} - {h.AssetName} ({h.Quantity} @ ${h.PurchasePrice:N2})").ToList();
        holdingChoices.Add("Cancel");

        var selectedHolding = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Select a holding to delete:[/]")
                .PageSize(15)
                .AddChoices(holdingChoices)
        );

        if (selectedHolding == "Cancel")
        {
            return;
        }

        var holdingId = int.Parse(selectedHolding.Split('.')[0]);
        var holding = holdings.First(h => h.Id == holdingId);

        var confirmed = AnsiConsole.Confirm(
            $"[red]Are you sure you want to delete {holding.Symbol}? This action cannot be undone.[/]",
            false
        );

        if (confirmed)
        {
            var deleted = await _portfolioService.DeleteHoldingAsync(holdingId);
            if (deleted)
            {
                DisplayFormatters.ShowSuccess($"Successfully deleted {holding.Symbol} from your portfolio.");
            }
            else
            {
                DisplayFormatters.ShowError("Failed to delete holding.");
            }
        }
        else
        {
            DisplayFormatters.ShowInfo("Deletion cancelled.");
        }

        DisplayFormatters.PressAnyKey();
    }
}