using Rudy.OrderLab;

internal static partial class Program
{
    private static int assertions;
    internal static void Assert(bool condition, string message)
    { assertions++; if (!condition) throw new Exception(message); }
    internal static void Throws(Action action, string message)
    { assertions++; try { action(); } catch (ArgumentException) { return; } throw new Exception(message); }
    internal static Operation Op(string id, long start, long end, string kind, string? result, params string[] args)
        => new(id, id, "main", start, end, kind, args, result);
    public static int Main()
    {
        Assert(new Limits().MaxStates > 0, "positive default state budget");
        ReaderTests();
        Console.WriteLine($"OrderLab: {assertions} assertions passed");
        return 0;
    }
}
