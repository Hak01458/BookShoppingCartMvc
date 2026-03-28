using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookShoppingCartMvcUI.Domain;
using System.Linq;

namespace BookShoppingCartMvcUI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CartItemCreator _itemCreator; // Factory Method creator
        private readonly Microsoft.Extensions.Logging.ILogger<CartRepository> _logger;

        public CartRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager, CartItemCreator itemCreator,
            Microsoft.Extensions.Logging.ILogger<CartRepository> logger)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _itemCreator = itemCreator ?? throw new ArgumentNullException(nameof(itemCreator));
            _logger = logger;
        }

        public async Task<int> DeleteItem(int bookId)
        {
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("user is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new InvalidOperationException("Invalid cart");
                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is null)
                    throw new InvalidOperationException("Not items in cart");
                _db.CartDetails.Remove(cartItem);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("user is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();
                // cart detail section
                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _db.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price  // it is a new line after update
                    };
                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "AddItem failed for BookId {BookId} Qty {Qty} User {UserId}", bookId, qty, userId);
                throw;
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }


        public async Task<int> RemoveItem(int bookId)
        {
            //using var transaction = _db.Database.BeginTransaction();
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("user is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new InvalidOperationException("Invalid cart");
                // cart detail section
                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is null)
                {
                    // no item to remove -> log and return current count instead of throwing
                    _logger?.LogWarning("RemoveItem: no cart detail for BookId {BookId} and User {UserId}", bookId, userId);
                    var currentCount = await GetCartItemCount(userId);
                    return currentCount;
                }
                else if (cartItem.Quantity == 1)
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity - 1;
                }
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "RemoveItem failed for BookId {BookId} User {UserId}", bookId, userId);
                // return current count to caller instead of throwing to UI
                var currentCount = await GetCartItemCount(userId);
                return currentCount;
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new InvalidOperationException("Invalid userid");
            var shoppingCart = await _db.ShoppingCarts
                                  .Include(a => a.CartDetails)
                                  .ThenInclude(a => a.Book)
                                  .ThenInclude(a => a.Stock)
                                  .Include(a => a.CartDetails)
                                  .ThenInclude(a => a.Book)
                                  .ThenInclude(a => a.Genre)
                                  .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;

        }
        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId)) // updated line
            {
                userId = GetUserId();
            }
            var data = await (from cart in _db.ShoppingCarts
                              join cartDetail in _db.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId==userId // updated line
                              select new { cartDetail.Id }
                        ).ToListAsync();
            return data.Count;
        }

        public async Task<(int OrderId, string UserId)> DoCheckout(CheckoutModel model)
        {
            string userId = GetUserId();
            try
            {
                // logic
                // move data from cartDetail to order and order detail then we will remove cart detail
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new InvalidOperationException("Invalid cart");
                var cartDetail = _db.CartDetails
                                    .Where(a => a.ShoppingCartId == cart.Id).ToList();
                if (cartDetail.Count == 0)
                    throw new InvalidOperationException("Cart is empty");
                // Visitor: build a bundle representing the cart and run stock check visitor
                var bundleForCheck = await BuildBundleFromCart(userId, "__cart_check__");
                var stockVisitor = new BookShoppingCartMvcUI.Domain.StockCheckVisitor(_db);
                bundleForCheck.Accept(stockVisitor);
                if (stockVisitor.Errors != null && stockVisitor.Errors.Count > 0)
                {
                    // return first error to caller
                    throw new InvalidOperationException(stockVisitor.Errors.First());
                }
                var pendingRecord = _db.orderStatuses.FirstOrDefault(s => s.StatusName == "Pending");
                if (pendingRecord is null)
                    throw new InvalidOperationException("Order status does not have Pending status");
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name=model.Name,
                    Email=model.Email,
                    MobileNumber=model.MobileNumber,
                    PaymentMethod=model.PaymentMethod,
                    Address=model.Address,
                    IsPaid=false,
                    OrderStatusId = pendingRecord.Id
                };
                _db.Orders.Add(order);
                _db.SaveChanges();
                foreach(var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _db.OrderDetails.Add(orderDetail);

                    // update stock here

                    var stock = await _db.Stocks.FirstOrDefaultAsync(a => a.BookId == item.BookId);
                    if (stock == null)
                    {
                        throw new InvalidOperationException("Stock is null");
                    }

                    if (item.Quantity > stock.Quantity)
                    {
                        throw new InvalidOperationException($"Only {stock.Quantity} items(s) are available in the stock");
                    }
                    // decrease the number of quantity from the stock table
                    stock.Quantity -= item.Quantity;
                }
                //_db.SaveChanges();

                // removing the cartdetails
                _db.CartDetails.RemoveRange(cartDetail);
                _db.SaveChanges();
                return (order.Id, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "DoCheckout failed for User {UserId}", userId);
                return (0, string.Empty);
            }
        }

        // -------------------- Composite helpers --------------------

        // Factory: convert a CartDetail -> BookLeaf
        private BookLeaf ConvertDetailToLeaf(CartDetail detail)
        {
            // use Factory Method (CartItemCreator) to create a cart item
            var created = _itemCreator.Create(detail);
            if (created is BookLeaf leaf)
                return leaf;

            // defensive fallback
            var name = detail.Book?.BookName ?? "Unknown";
            var unitPrice = Convert.ToDecimal(detail.UnitPrice);
            return new BookLeaf(detail.BookId, name, unitPrice, detail.Quantity);
        }

        // Build a bundle composite from user's cart items
        public async Task<BundleComposite> BuildBundleFromCart(string? userId, string bundleName)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("Invalid userid");

            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            if (cart is null)
                throw new InvalidOperationException("Invalid cart");

            var details = await _db.CartDetails
                                   .Include(d => d.Book)
                                   .Where(d => d.ShoppingCartId == cart.Id)
                                   .ToListAsync();

            var bundle = new BundleComposite(bundleName);

            foreach (var d in details)
            {
                var leaf = ConvertDetailToLeaf(d);
                bundle.AddChild(leaf);
            }

            return bundle;
        }

        // Demonstrate checkout flow for a BundleComposite (creates order, updates stock, removes cart items)
        public async Task<bool> CheckoutBundle(BundleComposite bundle, CheckoutModel model)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in");

                var pendingRecord = _db.orderStatuses.FirstOrDefault(s => s.StatusName == "Pending");
                if (pendingRecord is null)
                    throw new InvalidOperationException("Order status does not have Pending status");

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid = false,
                    OrderStatusId = pendingRecord.Id
                };
                _db.Orders.Add(order);
                _db.SaveChanges();

                // iterate children (expect BookLeaf)
                var bookLeafs = bundle.Children.OfType<BookLeaf>().ToList();
                foreach (var leaf in bookLeafs)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = leaf.BookId,
                        OrderId = order.Id,
                        Quantity = leaf.Quantity,
                        UnitPrice = Convert.ToDouble(leaf.Price)
                    };
                    _db.OrderDetails.Add(orderDetail);

                    // update stock
                    var stock = await _db.Stocks.FirstOrDefaultAsync(a => a.BookId == leaf.BookId);
                    if (stock == null)
                        throw new InvalidOperationException("Stock is null");
                    if (leaf.Quantity > stock.Quantity)
                        throw new InvalidOperationException($"Only {stock.Quantity} items(s) are available in the stock");
                    stock.Quantity -= leaf.Quantity;
                }

                // remove matching cart details for the books in the bundle
                var bookIds = bookLeafs.Select(b => b.BookId).Distinct().ToList();
                var cart = await GetCart(userId);
                if (cart != null)
                {
                    var detailsToRemove = _db.CartDetails.Where(d => d.ShoppingCartId == cart.Id && bookIds.Contains(d.BookId));
                    _db.CartDetails.RemoveRange(detailsToRemove);
                }

                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CheckoutBundle failed for user {UserId}", userId);
                throw;
            }
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }


    }
}
