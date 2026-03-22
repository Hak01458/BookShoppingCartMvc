using System.Collections.Generic;

namespace BookShoppingCartMvcUI.Domain
{
    public class BookLeaf : ICartItem
    {
        public BookLeaf(int bookId, string name, decimal price, int quantity = 1)
        {
            BookId = bookId;
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public int BookId { get; }
        public string Name { get; }
        public decimal Price { get; }
        public int Quantity { get; set; }

        public IReadOnlyCollection<ICartItem> Children => Array.Empty<ICartItem>();
        public void AddChild(ICartItem item) => throw new InvalidOperationException("Leaf cannot contain children");
        public void RemoveChild(ICartItem item) => throw new InvalidOperationException("Leaf has no children");

        public decimal GetTotalPrice() => Price * Quantity;
    }
}
