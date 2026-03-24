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
        Task<bool> DoCheckout(CheckoutModel model);

        // Prototype demo: duplicate a cart item using Clone()
        Task<int> DuplicateItem(int bookId);

        // Composite helpers
        Task<BundleComposite> BuildBundleFromCart(string? userId, string bundleName);
        Task<bool> CheckoutBundle(BundleComposite bundle, CheckoutModel model);
    }
}
