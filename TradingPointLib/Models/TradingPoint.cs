using System.ComponentModel;
using System.Runtime.CompilerServices;
using TradingPointLib.Events;

namespace TradingPointLib.Models;

public delegate void TradingEventHandler(object sender, TradingEventArgs e);

public class TradingPoint : INotifyPropertyChanged
{
    private string _name;
    private double _x;
    private double _y;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public double X
    {
        get => _x;
        set { _x = value; OnPropertyChanged(); }
    }

    public double Y
    {
        get => _y;
        set { _y = value; OnPropertyChanged(); }
    }

    public List<Product> Products { get; }
    public Queue<Customer> CustomerQueue { get; }
    public double BuyProbability { get; set; }

    public event TradingEventHandler? ProductPurchased;
    public event TradingEventHandler? ProductOutOfStock;

    public TradingPoint(string name, double x, double y, double buyProbability)
    {
        _name = name;
        _x = x;
        _y = y;
        BuyProbability = buyProbability;
        Products = new List<Product>();
        CustomerQueue = new Queue<Customer>();
    }

    public void AddProduct(Product product)
    {
        Products.Add(product);
    }

    public void EnqueueCustomer(Customer customer)
    {
        CustomerQueue.Enqueue(customer);
    }

    public void ServeNextCustomer(Random random)
    {
        if (CustomerQueue.Count == 0)
            return;

        var customer = CustomerQueue.Dequeue();

        double roll = random.NextDouble();
        if (roll >= BuyProbability)
        {
            return;
        }

        if (Products.Count == 0)
            return;

        int productIndex = random.Next(Products.Count);
        var product = Products[productIndex];

        if (product.Quantity > 0)
        {
            product.Quantity--;
            customer.Budget -= product.Price;

            OnProductPurchased(new TradingEventArgs(
                customer.Name,
                product.Name,
                Name,
                $"{customer.Name} купил(а) {product.Name} в «{Name}» за {product.Price:F2} руб."));
        }
        else
        {
            OnProductOutOfStock(new TradingEventArgs(
                customer.Name,
                product.Name,
                Name,
                $"Товар «{product.Name}» закончился в «{Name}»"));
        }
    }

    protected virtual void OnProductPurchased(TradingEventArgs e)
    {
        ProductPurchased?.Invoke(this, e);
    }

    protected virtual void OnProductOutOfStock(TradingEventArgs e)
    {
        ProductOutOfStock?.Invoke(this, e);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
