using BookShoppingCartMvcUI.Domain;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int bookId, int qty);
        Task<int> RemoveItem(int bookId);
        Task<ShoppingCart> GetUserCart();
        Task<ShoppingCart> GetCart(string userId);
        Task<int> GetCartItemCount(string userId = "");
        // returns (OrderId, UserId). OrderId > 0 means success.
        Task<(int OrderId, string UserId)> DoCheckout(CheckoutModel model);

        // Composite helpers
        Task<BundleComposite> BuildBundleFromCart(string? userId, string bundleName);
        Task<bool> CheckoutBundle(BundleComposite bundle, CheckoutModel model);
    }
}
