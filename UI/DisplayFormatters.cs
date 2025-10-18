using Spectre.Console;
using InvestmentPortfolioTracker.Models;
using InvestmentPortfolioTracker.Services;

namespace InvestmentPortfolioTracker.UI;

public static class DisplayFormatters
{
    public static void ShowHeader()
    {
        AnsiConsole.Clear();
        var rule = new Rule("[bold cyan]INVESTMENT PORTFOLIO TRACKER[/]");
        rule.Style = Style.Parse("cyan");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    public static void ShowHoldingsTable(IEnumerable<Holding> holdings)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(new TableColumn("[bold]ID[/]").Centered());
        table.AddColumn("[bold]Symbol[/]");
        table.AddColumn("[bold]Asset Name[/]");
        table.AddColumn("[bold]Type[/]");
        table.AddColumn(new TableColumn("[bold]Quantity[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Purchase Price[/]").RightAligned());
        table.AddColumn("[bold]Purchase Date[/]");
        table.AddColumn(new TableColumn("[bold]Current Price[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Current Value[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Gain/Loss[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Gain/Loss %[/]").RightAligned());

        foreach (var holding in holdings)
        {
            var gainLossColor = holding.GainLoss.HasValue && holding.GainLoss.Value >= 0 ? "green" : "red";
            
            table.AddRow(
                holding.Id.ToString(),
                $"[bold]{holding.Symbol}[/]",
                holding.AssetName,
                holding.AssetType,
                holding.Quantity.ToString("N4"),
                $"${holding.PurchasePrice:N2}",
                holding.PurchaseDate.ToString("yyyy-MM-dd"),
                holding.CurrentPrice.HasValue ? $"${holding.CurrentPrice.Value:N2}" : "[dim]N/A[/]",
                holding.CurrentValue.HasValue ? $"${holding.CurrentValue.Value:N2}" : "[dim]N/A[/]",
                holding.GainLoss.HasValue ? $"[{gainLossColor}]${holding.GainLoss.Value:N2}[/]" : "[dim]N/A[/]",
                holding.GainLossPercent.HasValue ? $"[{gainLossColor}]{holding.GainLossPercent.Value:N2}%[/]" : "[dim]N/A[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    public static void ShowPortfolioSummary(PortfolioSummary summary)
    {
        var panel = new Panel(
            new Markup(
                $"[bold]Total Invested:[/] [cyan]${summary.TotalInvested:N2}[/]\n" +
                $"[bold]Current Value:[/] [cyan]${summary.CurrentValue:N2}[/]\n" +
                $"[bold]Total Gain/Loss:[/] {(summary.TotalGainLoss >= 0 ? "[green]" : "[red]")}${summary.TotalGainLoss:N2} ({summary.TotalGainLossPercent:N2}%)[/]\n" +
                $"[bold]Holdings:[/] {summary.HoldingsCount} total, {summary.HoldingsWithPrices} with prices"
            )
        );
        panel.Header = new PanelHeader("[bold yellow]Portfolio Summary[/]");
        panel.Border = BoxBorder.Rounded;
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        if (summary.TopHoldings.Any())
        {
            AnsiConsole.MarkupLine("[bold underline]Top Holdings by Value:[/]");
            var topTable = new Table();
            topTable.Border(TableBorder.Minimal);
            topTable.AddColumn("[bold]Symbol[/]");
            topTable.AddColumn(new TableColumn("[bold]Current Value[/]").RightAligned());
            topTable.AddColumn(new TableColumn("[bold]Gain/Loss[/]").RightAligned());
            topTable.AddColumn(new TableColumn("[bold]%[/]").RightAligned());

            foreach (var holding in summary.TopHoldings)
            {
                var color = holding.GainLoss >= 0 ? "green" : "red";
                topTable.AddRow(
                    $"[bold]{holding.Symbol}[/]",
                    $"${holding.CurrentValue!.Value:N2}",
                    $"[{color}]${holding.GainLoss!.Value:N2}[/]",
                    $"[{color}]{holding.GainLossPercent!.Value:N2}%[/]"
                );
            }
            AnsiConsole.Write(topTable);
            AnsiConsole.WriteLine();
        }

        if (summary.BestPerformer != null)
        {
            AnsiConsole.MarkupLine($"[bold]Best Performer:[/] [green]{summary.BestPerformer.Symbol}[/] ([green]+{summary.BestPerformer.GainLossPercent!.Value:N2}%[/])");
        }

        if (summary.WorstPerformer != null)
        {
            AnsiConsole.MarkupLine($"[bold]Worst Performer:[/] [red]{summary.WorstPerformer.Symbol}[/] ([red]{summary.WorstPerformer.GainLossPercent!.Value:N2}%[/])");
        }
    }

    public static void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓[/] {message}");
    }

    public static void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]✗ Error:[/] {message}");
    }

    public static void ShowWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠[/] {message}");
    }

    public static void ShowInfo(string message)
    {
        AnsiConsole.MarkupLine($"[blue]ℹ[/] {message}");
    }

    public static void PressAnyKey()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}