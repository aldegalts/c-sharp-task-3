using System.Reflection;
using System.Text;

namespace TradingPointLib.Helpers;

public static class ReflectionHelper
{
    public static string GetClassInfo(Type type)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== Информация о классе: {type.Name} ===");
        sb.AppendLine($"Полное имя: {type.FullName}");
        sb.AppendLine($"Пространство имён: {type.Namespace}");
        sb.AppendLine($"Базовый класс: {type.BaseType?.Name}");

        var interfaces = type.GetInterfaces();
        if (interfaces.Length > 0)
        {
            sb.AppendLine("Интерфейсы:");
            foreach (var iface in interfaces)
                sb.AppendLine($"  - {iface.Name}");
        }

        var constructors = type.GetConstructors();
        sb.AppendLine($"Конструкторы ({constructors.Length}):");
        foreach (var ctor in constructors)
        {
            var parameters = ctor.GetParameters()
                .Select(p => $"{p.ParameterType.Name} {p.Name}");
            sb.AppendLine($"  ({string.Join(", ", parameters)})");
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        sb.AppendLine($"Свойства ({properties.Length}):");
        foreach (var prop in properties)
            sb.AppendLine($"  {prop.PropertyType.Name} {prop.Name}");

        var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance);
        if (events.Length > 0)
        {
            sb.AppendLine($"События ({events.Length}):");
            foreach (var ev in events)
                sb.AppendLine($"  {ev.EventHandlerType?.Name} {ev.Name}");
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName);
        var methodList = methods.ToList();
        if (methodList.Count > 0)
        {
            sb.AppendLine($"Методы ({methodList.Count}):");
            foreach (var method in methodList)
            {
                var parameters = method.GetParameters()
                    .Select(p => $"{p.ParameterType.Name} {p.Name}");
                sb.AppendLine($"  {method.ReturnType.Name} {method.Name}({string.Join(", ", parameters)})");
            }
        }

        return sb.ToString();
    }

    public static T CreateInstance<T>(params object[] args)
    {
        var instance = Activator.CreateInstance(typeof(T), args);
        return (T)instance!;
    }

    public static IEnumerable<Type> GetImplementations<TInterface>()
    {
        var interfaceType = typeof(TInterface);
        var assembly = interfaceType.Assembly;

        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));
    }
}
