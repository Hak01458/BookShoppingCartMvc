using MediatR;

namespace BookShoppingCartMvcUI.Features.Notifications
{
    public record OrderPlacedNotification(int OrderId, string UserId) : INotification;
}
