using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TradingPointLib.Models;

public class Customer : INotifyPropertyChanged
{
    private string _name;
    private decimal _budget;
    private double _x;
    private double _y;
    private bool _isServed;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public decimal Budget
    {
        get => _budget;
        set { _budget = value; OnPropertyChanged(); }
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

    public double TargetX { get; set; }
    public double TargetY { get; set; }
    public double Speed { get; set; }

    public bool IsServed
    {
        get => _isServed;
        set { _isServed = value; OnPropertyChanged(); }
    }

    public Customer(string name, decimal budget, double x, double y, double speed)
    {
        _name = name;
        _budget = budget;
        _x = x;
        _y = y;
        Speed = speed;
    }

    public bool MoveTowardTarget()
    {
        double dx = TargetX - X;
        double dy = TargetY - Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance < Speed)
        {
            X = TargetX;
            Y = TargetY;
            return true;
        }

        X += (dx / distance) * Speed;
        Y += (dy / distance) * Speed;
        return false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
