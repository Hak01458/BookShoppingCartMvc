using System.Collections.Generic;
using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Data;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Domain
{
    // Visitor that checks stock availability for BookLeaf items.
    public class StockCheckVisitor : ICartVisitor
    {
        private readonly ApplicationDbContext _db;
        private readonly List<string> _errors = new();

        public IReadOnlyCollection<string> Errors => _errors.AsReadOnly();

        public StockCheckVisitor(ApplicationDbContext db)
        {
            _db = db;
        }

        public void Visit(BookLeaf leaf)
        {
            var stock = _db.Stocks.FirstOrDefault(s => s.BookId == leaf.BookId);
            if (stock == null)
            {
                _errors.Add($"Stock not found for book id {leaf.BookId}");
                return;
            }
            if (leaf.Quantity > stock.Quantity)
            {
                _errors.Add($"Only {stock.Quantity} item(s) available for '{leaf.Name}'");
            }
        }

        public void Visit(BundleComposite composite)
        {
            // nothing special at composite level for stock; children will be visited
        }
    }
}
