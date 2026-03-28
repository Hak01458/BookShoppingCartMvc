using MediatR;
using BookShoppingCartMvcUI.Facades;
using BookShoppingCartMvcUI.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Features.Cart
{
    public class GetUserCartHandler : IRequestHandler<GetUserCartQuery, ShoppingCart>
    {
        private readonly ICartFacade _cartFacade;

        public GetUserCartHandler(ICartFacade cartFacade)
        {
            _cartFacade = cartFacade;
        }

        public Task<ShoppingCart> Handle(GetUserCartQuery request, CancellationToken cancellationToken)
        {
            return _cartFacade.GetUserCartAsync();
        }
    }
}
