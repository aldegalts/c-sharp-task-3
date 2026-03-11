using TradingPointLib.Models;

namespace TradingPointLib.Interfaces;

public interface IDeliveryService
{
    Task DeliverAsync(TradingPoint point, string productName, int quantity,
        CancellationToken cancellationToken = default);

    event Action<string>? DeliveryCompleted;
}
