using MediatR;
using BookShoppingCartMvcUI.Facades;
using System.Threading;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Features.Cart
{
    public class GetCartItemCountHandler : IRequestHandler<GetCartItemCountQuery, int>
    {
        private readonly ICartFacade _cartFacade;

        public GetCartItemCountHandler(ICartFacade cartFacade)
        {
            _cartFacade = cartFacade;
        }

        public Task<int> Handle(GetCartItemCountQuery request, CancellationToken cancellationToken)
        {
            return _cartFacade.GetCartItemCountAsync();
        }
    }
}
