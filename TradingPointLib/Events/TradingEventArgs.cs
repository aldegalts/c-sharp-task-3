namespace TradingPointLib.Events;

public class TradingEventArgs : EventArgs
{
    public string CustomerName { get; }
    public string ProductName { get; }
    public string TradingPointName { get; }
    public string Message { get; }
    public DateTime Timestamp { get; }

    public TradingEventArgs(string customerName, string productName,
        string tradingPointName, string message)
    {
        CustomerName = customerName;
        ProductName = productName;
        TradingPointName = tradingPointName;
        Message = message;
        Timestamp = DateTime.Now;
    }
}
