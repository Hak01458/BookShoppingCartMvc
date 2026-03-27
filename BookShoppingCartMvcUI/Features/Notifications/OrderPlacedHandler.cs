using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Features.Notifications
{
    public class OrderPlacedHandler : INotificationHandler<OrderPlacedNotification>
    {
        private readonly ILogger<OrderPlacedHandler> _logger;
        private readonly IMemoryCache _cache;

        public OrderPlacedHandler(ILogger<OrderPlacedHandler> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(notification.UserId))
            {
                var cacheKey = $"cart_{notification.UserId}";
                _cache.Remove(cacheKey);
            }
            _logger.LogInformation("OrderPlacedNotification handled for Order {OrderId} user {UserId}", notification.OrderId, notification.UserId);
            return Task.CompletedTask;
        }
    }
}
