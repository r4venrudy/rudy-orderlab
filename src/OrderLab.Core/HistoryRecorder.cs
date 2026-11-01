using System.Globalization;
namespace Rudy.OrderLab;
/// <summary>One recorder supplies a process-local globally ordered invocation/response clock.</summary>
public sealed class HistoryRecorder
{
    private readonly object gate=new();
    private readonly string model;
    private readonly string[] initial;
    private long ticks;
    private readonly Dictionary<string,Operation> pending=new(StringComparer.Ordinal);
    private readonly List<Operation> completed=[];
    public HistoryRecorder(string model,params string[] initial)
    { this.model=model;this.initial=(string[])initial.Clone();_ = Models.Get(model).Initial(initial); }
    public string Begin(string client,string objectName,string kind,params string[] arguments)
    {
        lock(gate)
        {
            if(pending.Count+completed.Count>=HistoryReader.MaxOperations)throw new ArgumentException("Recorder operation limit.");
            if(pending.Values.Any(o=>o.Client==client))throw new ArgumentException("Client already has a pending operation.");
            var tick=ticks++;var id="op-"+tick.ToString(CultureInfo.InvariantCulture);
            pending.Add(id,new(id,client,objectName,tick,-1,kind,(string[])arguments.Clone(),null));return id;
        }
    }
    public void Complete(string id,string? result)
    {
        lock(gate)
        {
            if(!pending.TryGetValue(id,out var op))throw new ArgumentException("Invocation is missing or already completed.");
            var finished=op with{End=ticks++,Result=result};Models.Get(model).Validate(finished);
            completed.Add(finished);pending.Remove(id);
        }
    }
    public History Snapshot()
    {
        lock(gate)
        {
            if(pending.Count!=0)throw new ArgumentException("Pending operations cannot be silently omitted from a complete history.");
            var h=new History(1,model,(string[])initial.Clone(),completed.OrderBy(o=>o.Start).Select(o=>o with{Args=(string[])o.Args.Clone()}).ToArray());
            HistoryReader.Validate(h);return h;
        }
    }
}
