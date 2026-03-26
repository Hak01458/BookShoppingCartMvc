using BookShoppingCartMvcUI.Domain;
using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Facades
{
    public class CartFacade : ICartFacade
    {
        private readonly ICartRepository _cartRepo;
        private readonly IStockRepository _stockRepo;
        private readonly ILogger<CartFacade> _logger;

        public CartFacade(ICartRepository cartRepo, IStockRepository stockRepo, ILogger<CartFacade> logger)
        {
            _cartRepo = cartRepo ?? throw new ArgumentNullException(nameof(cartRepo));
            _stockRepo = stockRepo ?? throw new ArgumentNullException(nameof(stockRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<int> AddItemAsync(int bookId, int qty = 1)
        {
            _logger.LogInformation("Adding item {BookId} qty {Qty}", bookId, qty);
            return _cartRepo.AddItem(bookId, qty);
        }

        public Task<int> RemoveItemAsync(int bookId)
        {
            _logger.LogInformation("Removing item {BookId}", bookId);
            return _cartRepo.RemoveItem(bookId);
        }

        public Task<ShoppingCart> GetUserCartAsync()
        {
            _logger.LogDebug("Getting user cart");
            return _cartRepo.GetUserCart();
        }

        public Task<int> GetCartItemCountAsync()
        {
            _logger.LogDebug("Getting cart item count");
            return _cartRepo.GetCartItemCount();
        }

        public async Task<bool> CheckoutAsync(CheckoutModel model)
        {
            _logger.LogInformation("Starting checkout for user");
            var ok = await _cartRepo.DoCheckout(model);
            if (ok)
            {
                _logger.LogInformation("Checkout succeeded");
            }
            else
            {
                _logger.LogWarning("Checkout failed");
            }
            return ok;
        }

        public Task<BundleComposite> BuildBundleFromCartAsync(string? userId, string bundleName)
        {
            _logger.LogDebug("Building bundle {BundleName} for user {UserId}", bundleName, userId ?? "(current)");
            return _cartRepo.BuildBundleFromCart(userId, bundleName);
        }

        public async Task<bool> CheckoutBundleAsync(BundleComposite bundle, CheckoutModel model)
        {
            _logger.LogInformation("Checking out bundle {BundleName}", bundle?.Name);

            if (bundle == null) throw new ArgumentNullException(nameof(bundle));

            // pre-check stock availability using iterator to traverse children
            var iterator = bundle.CreateIterator();
            foreach (var item in iterator.AsEnumerable())
            {
                if (item is BookLeaf leaf)
                {
                    var stock = await _stockRepo.GetStockByBookId(leaf.BookId);
                    if (stock == null)
                    {
                        _logger.LogWarning("Stock not found for BookId {BookId}", leaf.BookId);
                        return false;
                    }
                    if (leaf.Quantity > stock.Quantity)
                    {
                        _logger.LogWarning("Insufficient stock for BookId {BookId}: requested {Req} available {Avail}", leaf.BookId, leaf.Quantity, stock.Quantity);
                        return false;
                    }
                }
            }

            // delegate actual checkout to repository (it will perform its own checks/transaction)
            return await _cartRepo.CheckoutBundle(bundle, model);
        }
    }
}
