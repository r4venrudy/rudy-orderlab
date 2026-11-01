namespace Rudy.OrderLab;

/// <summary>Ticks are globally comparable logical event positions, not wall clocks.</summary>
public sealed record Operation(string Id, string Client, string Object, long Start, long End,
    string Kind, string[] Args, string? Result);
public sealed record History(int Schema, string Model, string[] Initial, Operation[] Operations);
public enum Verdict { Linearizable, NotLinearizable, Inconclusive }
public sealed record Limits(long MaxStates = 1_000_000, int MaxMilliseconds = 10_000);
public sealed record Rejection(string OperationId, string Reason);
public sealed record CheckResult(Verdict Verdict, string Reason, string[] Witness,
    string[] Frontier, Rejection[] Rejections, long States, long Transitions, long CacheHits);
public interface IModel
{
    string Name { get; }
    string Initial(string[] values);
    void Validate(Operation operation);
    bool Apply(string state, Operation operation, out string next, out string reason);
}
