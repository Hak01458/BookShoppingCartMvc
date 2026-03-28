using System;
using System.Collections.Generic;

namespace BookShoppingCartMvcUI.Domain
{
    public class BookLeaf : ICartItem
    {
        public BookLeaf(int bookId, string name, decimal price, int quantity = 1, string? authorName = null, string? genreName = null, string? image = null)
        {
            BookId = bookId;
            Name = name;
            Price = price;
            Quantity = quantity;
            AuthorName = authorName;
            GenreName = genreName;
            Image = image;
        }

        public int BookId { get; }
        public string Name { get; }
        public decimal Price { get; }
        public int Quantity { get; set; }
        public string? AuthorName { get; }
        public string? GenreName { get; }
        public string? Image { get; }

        public IReadOnlyCollection<ICartItem> Children => Array.Empty<ICartItem>();
        public void AddChild(ICartItem item) => throw new InvalidOperationException("Leaf cannot contain children");
        public void RemoveChild(ICartItem item) => throw new InvalidOperationException("Leaf has no children");

        public decimal GetTotalPrice() => Price * Quantity;

        public ICartItem Clone()
        {
            // BookLeaf has only value types / immutable data, shallow copy is sufficient
            return new BookLeaf(BookId, Name, Price, Quantity);
        }

        public void Accept(ICartVisitor visitor)
        {
            visitor.Visit(this);
        }

        public ICartIterator CreateIterator()
        {
            return new CartIterator(new List<ICartItem>());
        }
    }
}
