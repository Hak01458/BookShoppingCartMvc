using MediatR;
using BookShoppingCartMvcUI.Models.DTOs;

namespace BookShoppingCartMvcUI.Features.Cart
{
    public record CheckoutCommand(CheckoutModel Model) : IRequest<bool>;
}
