using MediatR;
using BookShoppingCartMvcUI.Facades;
using BookShoppingCartMvcUI.Models.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Features.Cart
{
    public class CheckoutHandler : IRequestHandler<CheckoutCommand, bool>
    {
        private readonly ICartFacade _cartFacade;

        public CheckoutHandler(ICartFacade cartFacade)
        {
            _cartFacade = cartFacade;
        }

        public Task<bool> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            return _cartFacade.CheckoutAsync(request.Model);
        }
    }
}
