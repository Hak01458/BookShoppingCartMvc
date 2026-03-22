using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Domain;

namespace BookShoppingCartMvcUI.Domain
{
    // Abstract Creator defining the Factory Method
    public abstract class CartItemCreator
    {
        // Factory Method - subclasses override to create specific ICartItem implementations
        public abstract ICartItem Create(CartDetail detail);
    }
}
