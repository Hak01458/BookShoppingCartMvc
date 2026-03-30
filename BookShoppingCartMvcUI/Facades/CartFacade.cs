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
        private readonly IStockRepository _stock_repo;
        private readonly ILogger<CartFacade> _logger;
        private readonly MediatR.IMediator _mediator;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        public CartFacade(ICartRepository cartRepo, IStockRepository stockRepo, ILogger<CartFacade> logger, MediatR.IMediator mediator, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            _cartRepo = cartRepo ?? throw new ArgumentNullException(nameof(cartRepo));
            _stock_repo = stockRepo ?? throw new ArgumentNullException(nameof(stockRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator;
            _cache = cache;
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

        public Task<int> DeleteItemAsync(int bookId)
        {
            _logger.LogInformation("Deleting item {BookId}", bookId);
            return _cartRepo.DeleteItem(bookId);
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
            var (orderId, userId) = await _cartRepo.DoCheckout(model);
            if (orderId > 0)
            {
                _logger.LogInformation("Checkout succeeded, order {OrderId}", orderId);
                try
                {
                    // publish order placed notification with actual user id
                    await _mediator.Publish(new BookShoppingCartMvcUI.Features.Notifications.OrderPlacedNotification(orderId, userId));
                }
                catch { }
                return true;
            }
            _logger.LogWarning("Checkout failed");
            return false;
        }

        public Task<BundleComposite> BuildBundleFromCartAsync(string? userId, string bundleName)
        {
            _logger.LogDebug("Building bundle {BundleName} for user {UserId}", bundleName, userId ?? "(current)");
            return _cartRepo.BuildBundleFromCart(userId, bundleName);
        }
        public async Task<int> DeleteItemAsync(int bookId)
        {
            // delegate to repository implementation
            return await _cartRepo.DeleteItem(bookId);
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
                    var stock = await _stock_repo.GetStockByBookId(leaf.BookId);
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
