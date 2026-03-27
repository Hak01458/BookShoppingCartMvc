using MediatR;
using BookShoppingCartMvcUI.Domain;

namespace BookShoppingCartMvcUI.Features.Cart
{
    public record GetUserCartQuery() : IRequest<ShoppingCart>;
}
