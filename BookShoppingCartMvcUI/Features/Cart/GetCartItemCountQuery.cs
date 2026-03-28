using MediatR;

namespace BookShoppingCartMvcUI.Features.Cart
{
    public record GetCartItemCountQuery() : IRequest<int>;
}
