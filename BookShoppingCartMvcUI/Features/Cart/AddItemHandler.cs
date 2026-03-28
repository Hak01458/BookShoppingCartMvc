using MediatR;
using BookShoppingCartMvcUI.Facades;
using System.Threading;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Features.Cart
{
    public class AddItemHandler : IRequestHandler<AddItemCommand, int>
    {
        private readonly ICartFacade _cartFacade;

        public AddItemHandler(ICartFacade cartFacade)
        {
            _cartFacade = cartFacade;
        }

        public Task<int> Handle(AddItemCommand request, CancellationToken cancellationToken)
        {
            return _cartFacade.AddItemAsync(request.BookId, request.Quantity);
        }
    }
}
