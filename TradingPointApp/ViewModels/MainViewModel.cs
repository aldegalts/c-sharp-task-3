using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TradingPointLib.Events;
using TradingPointLib.Helpers;
using TradingPointLib.Interfaces;
using TradingPointLib.Models;
using TradingPointLib.Services;

namespace TradingPointApp.ViewModels;

public class MainViewModel : BaseViewModel
{
    private static readonly string[] ProductNames = { "Хлеб", "Молоко", "Яблоки", "Сыр", "Масло" };
    private static readonly decimal[] ProductPrices = { 45m, 80m, 120m, 250m, 95m };
    private static readonly string[] CustomerNames =
    {
        "Анна", "Борис", "Вера", "Григорий", "Дарья",
        "Евгений", "Жанна", "Захар", "Ирина", "Кирилл"
    };

    private readonly Random _random = new();
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private readonly IDeliveryService _deliveryService;
    private int _tradingPointCounter;
    private int _customerCounter;

    public ObservableCollection<TradingPoint> TradingPoints { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<string> LogMessages { get; } = new();

    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }

    public ICommand AddTradingPointCommand { get; }
    public ICommand AddCustomerCommand { get; }
    public ICommand StartSimulationCommand { get; }
    public ICommand StopSimulationCommand { get; }
    public ICommand ShowReflectionInfoCommand { get; }

    public double CanvasWidth { get; set; } = 700;
    public double CanvasHeight { get; set; } = 500;

    public MainViewModel()
    {
        _deliveryService = ReflectionHelper.CreateInstance<DeliveryService>(3000);
        _deliveryService.DeliveryCompleted += OnDeliveryCompleted;

        AddTradingPointCommand = new RelayCommand(_ => AddTradingPoint(), _ => true);
        AddCustomerCommand = new RelayCommand(_ => AddCustomer(), _ => TradingPoints.Count > 0);
        StartSimulationCommand = new RelayCommand(_ => StartSimulation(), _ => !IsRunning);
        StopSimulationCommand = new RelayCommand(_ => StopSimulation(), _ => IsRunning);
        ShowReflectionInfoCommand = new RelayCommand(_ => ShowReflectionInfo(), _ => true);
    }

    private void AddTradingPoint()
    {
        _tradingPointCounter++;
        double x = _random.Next(80, (int)CanvasWidth - 80);
        double y = _random.Next(80, (int)CanvasHeight - 80);

        var point = new TradingPoint($"Магазин #{_tradingPointCounter}", x, y, 0.75);

        for (int i = 0; i < ProductNames.Length; i++)
        {
            int quantity = _random.Next(3, 10);
            point.AddProduct(new Product(ProductNames[i], ProductPrices[i], quantity));
        }

        point.ProductPurchased += OnProductPurchased;
        point.ProductOutOfStock += OnProductOutOfStock;

        TradingPoints.Add(point);
        AddLog($"Торговая точка «{point.Name}» добавлена ({x:F0}; {y:F0})");
    }

    private void AddCustomer()
    {
        if (TradingPoints.Count == 0)
            return;

        _customerCounter++;
        string name = CustomerNames[_random.Next(CustomerNames.Length)] + $" #{_customerCounter}";
        decimal budget = _random.Next(100, 1000);
        double speed = 1.5 + _random.NextDouble() * 2.0;

        int side = _random.Next(4);
        double startX, startY;
        switch (side)
        {
            case 0: startX = 0; startY = _random.Next((int)CanvasHeight); break;
            case 1: startX = CanvasWidth; startY = _random.Next((int)CanvasHeight); break;
            case 2: startX = _random.Next((int)CanvasWidth); startY = 0; break;
            default: startX = _random.Next((int)CanvasWidth); startY = CanvasHeight; break;
        }

        var customer = new Customer(name, budget, startX, startY, speed);

        var targetPoint = TradingPoints[_random.Next(TradingPoints.Count)];
        customer.TargetX = targetPoint.X + 20;
        customer.TargetY = targetPoint.Y + 20;

        Customers.Add(customer);
        AddLog($"Покупатель «{name}» появился (бюджет: {budget:F0} руб.)");
    }

    private async void StartSimulation()
    {
        _cts = new CancellationTokenSource();
        IsRunning = true;
        AddLog("Симуляция запущена");

        try
        {
            await SimulationLoopAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void StopSimulation()
    {
        _cts?.Cancel();
        AddLog("Симуляция остановлена");
    }

    private async Task SimulationLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            Application.Current.Dispatcher.Invoke(() => UpdateSimulation());
            await Task.Delay(50, ct);
        }
    }

    private void UpdateSimulation()
    {
        var servedCustomers = new List<Customer>();

        foreach (var customer in Customers.ToList())
        {
            if (customer.IsServed)
            {
                servedCustomers.Add(customer);
                continue;
            }

            bool arrived = customer.MoveTowardTarget();
            if (arrived)
            {
                var targetPoint = TradingPoints
                    .FirstOrDefault(tp =>
                        Math.Abs(tp.X + 20 - customer.TargetX) < 5 &&
                        Math.Abs(tp.Y + 20 - customer.TargetY) < 5);

                if (targetPoint != null)
                {
                    targetPoint.EnqueueCustomer(customer);
                    targetPoint.ServeNextCustomer(_random);
                }

                customer.IsServed = true;
            }
        }

        foreach (var served in servedCustomers)
        {
            Customers.Remove(served);
        }

        if (IsRunning && _random.NextDouble() < 0.02 && TradingPoints.Count > 0)
        {
            AddCustomer();
        }
    }

    private void OnProductPurchased(object sender, TradingEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AddLog($"ПОКУПКА: {e.Message}");
        });
    }

    private void OnProductOutOfStock(object sender, TradingEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AddLog($"НЕТ В НАЛИЧИИ: {e.Message}");
        });

        if (sender is TradingPoint point)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _deliveryService.DeliverAsync(point, e.ProductName, 5,
                        _cts?.Token ?? CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                }
            });
        }
    }

    private void OnDeliveryCompleted(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AddLog($"ДОСТАВКА: {message}");
        });
    }

    private void ShowReflectionInfo()
    {
        var info = ReflectionHelper.GetClassInfo(typeof(TradingPoint));
        AddLog(info);

        var implementations = ReflectionHelper.GetImplementations<IDeliveryService>();
        foreach (var type in implementations)
        {
            AddLog($"Реализация IDeliveryService: {type.FullName}");
        }
    }

    private void AddLog(string message)
    {
        LogMessages.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
