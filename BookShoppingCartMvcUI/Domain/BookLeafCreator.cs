using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Domain;

namespace BookShoppingCartMvcUI.Domain
{
    // Concrete Creator that produces BookLeaf instances
    public class BookLeafCreator : CartItemCreator
    {
        public override ICartItem Create(CartDetail detail)
        {
            var name = detail.Book?.BookName ?? "Unknown";
            var unitPrice = Convert.ToDecimal(detail.UnitPrice);
            return new BookLeaf(detail.BookId, name, unitPrice, detail.Quantity);
        }
    }
}
