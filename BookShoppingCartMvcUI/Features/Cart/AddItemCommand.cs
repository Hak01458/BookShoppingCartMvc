using MediatR;

namespace BookShoppingCartMvcUI.Features.Cart
{
    public record AddItemCommand(int BookId, int Quantity = 1) : IRequest<int>;
}
