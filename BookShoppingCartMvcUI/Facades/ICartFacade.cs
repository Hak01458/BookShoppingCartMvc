using BookShoppingCartMvcUI.Domain;
using BookShoppingCartMvcUI.Models;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Facades
{
    public interface ICartFacade
    {
        Task<int> AddItemAsync(int bookId, int qty = 1);
        Task<int> RemoveItemAsync(int bookId);
        Task<int> DeleteItemAsync(int bookId);
        Task<ShoppingCart> GetUserCartAsync();
        Task<int> GetCartItemCountAsync();
        Task<bool> CheckoutAsync(CheckoutModel model);
        Task<int> DeleteItemAsync(int bookId);

        Task<BundleComposite> BuildBundleFromCartAsync(string? userId, string bundleName);
        Task<bool> CheckoutBundleAsync(BundleComposite bundle, CheckoutModel model);
    }
}
