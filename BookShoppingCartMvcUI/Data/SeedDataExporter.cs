using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookShoppingCartMvcUI.Data
{
    /// <summary>
    /// Utility to export and import selected database tables to/from a JSON file.
    /// Use ExportAsync to create a `seeddata.json` file from your local DB and commit it to the repo.
    /// Teammates can then call ImportAsync to populate their DB with the same data.
    /// </summary>
    public static class SeedDataExporter
    {
        public static async Task ExportAsync(IServiceProvider services, string fileName = "seeddata.json")
        {
            var context = services.GetService<ApplicationDbContext>();
            if (context == null) throw new InvalidOperationException("ApplicationDbContext not available from service provider");

            var data = new
            {
                Genres = await context.Genres.AsNoTracking().ToListAsync(),
                Books = await context.Books.AsNoTracking().ToListAsync(),
                Stocks = await context.Stocks.AsNoTracking().ToListAsync(),
                OrderStatuses = await context.orderStatuses.AsNoTracking().ToListAsync()
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            await File.WriteAllTextAsync(path, json);
        }

        public static async Task ImportAsync(IServiceProvider services, string fileName = "seeddata.json")
        {
            var context = services.GetService<ApplicationDbContext>();
            if (context == null) throw new InvalidOperationException("ApplicationDbContext not available from service provider");

            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            if (!File.Exists(path)) throw new FileNotFoundException("Seed file not found", path);

            var json = await File.ReadAllTextAsync(path);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Genres
            if (root.TryGetProperty("Genres", out var genresElement) && !context.Genres.Any())
            {
                var genres = JsonSerializer.Deserialize<List<BookShoppingCartMvcUI.Models.Genre>>(genresElement.GetRawText());
                if (genres != null)
                {
                    await context.Genres.AddRangeAsync(genres);
                    await context.SaveChangesAsync();
                }
            }

            // Books
            if (root.TryGetProperty("Books", out var booksElement) && !context.Books.Any())
            {
                var books = JsonSerializer.Deserialize<List<BookShoppingCartMvcUI.Models.Book>>(booksElement.GetRawText());
                if (books != null)
                {
                    await context.Books.AddRangeAsync(books);
                    await context.SaveChangesAsync();
                }
            }

            // Stocks
            if (root.TryGetProperty("Stocks", out var stocksElement) && !context.Stocks.Any())
            {
                var stocks = JsonSerializer.Deserialize<List<BookShoppingCartMvcUI.Models.Stock>>(stocksElement.GetRawText());
                if (stocks != null)
                {
                    await context.Stocks.AddRangeAsync(stocks);
                    await context.SaveChangesAsync();
                }
            }

            // OrderStatuses
            if (root.TryGetProperty("OrderStatuses", out var statusElement) && !context.orderStatuses.Any())
            {
                var statuses = JsonSerializer.Deserialize<List<BookShoppingCartMvcUI.Models.OrderStatus>>(statusElement.GetRawText());
                if (statuses != null)
                {
                    await context.orderStatuses.AddRangeAsync(statuses);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
