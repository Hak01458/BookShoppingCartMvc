using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly BookShoppingCartMvcUI.Facades.ICartFacade _cartFacade;
        private readonly MediatR.IMediator _mediator;

        public CartController(BookShoppingCartMvcUI.Facades.ICartFacade cartFacade, MediatR.IMediator mediator)
        {
            _cartFacade = cartFacade;
            _mediator = mediator;
        }
        public async Task<IActionResult> AddItem(int bookId, int qty = 1, int redirect = 0)
        {
            var cartCount = await _mediator.Send(new Features.Cart.AddItemCommand(bookId, qty));
            if (redirect == 0)
                return Ok(cartCount);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> RemoveItem(int bookId)
        {
            var cartCount = await _cartFacade.RemoveItemAsync(bookId);
            return RedirectToAction("GetUserCart");
        }
        public async Task<IActionResult> DeleteItem(int bookId)
        {
            var cartCount = await _cartFacade.DeleteItem(bookId);
            return RedirectToAction("GetUserCart");
        }
        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _mediator.Send(new Features.Cart.GetUserCartQuery());
            return View(cart);
        }

        public  async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await _mediator.Send(new Features.Cart.GetCartItemCountQuery());
            return Ok(cartItem);
        }

        public  IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            bool isCheckedOut = await _mediator.Send(new Features.Cart.CheckoutCommand(model));
            if (!isCheckedOut)
                return RedirectToAction(nameof(OrderFailure));
            return RedirectToAction(nameof(OrderSuccess));
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }

        public IActionResult OrderFailure()
        {
            return View();
        }

    }
}
