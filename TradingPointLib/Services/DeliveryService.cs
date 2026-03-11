using TradingPointLib.Interfaces;
using TradingPointLib.Models;

namespace TradingPointLib.Services;

public class DeliveryService : IDeliveryService
{
    private readonly int _deliveryDelayMs;

    public event Action<string>? DeliveryCompleted;

    public DeliveryService(int deliveryDelayMs)
    {
        _deliveryDelayMs = deliveryDelayMs;
    }

    public async Task DeliverAsync(TradingPoint point, string productName, int quantity,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(_deliveryDelayMs, cancellationToken);

        var product = point.Products.FirstOrDefault(p => p.Name == productName);
        if (product != null)
        {
            product.Quantity += quantity;
            DeliveryCompleted?.Invoke(
                $"Доставка в «{point.Name}»: {productName} +{quantity} шт.");
        }
    }
}
