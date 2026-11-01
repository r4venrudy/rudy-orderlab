namespace Rudy.OrderLab;

public static class PartitionChecker
{
    public static CheckResult Check(History h,IModel model,Limits? limits=null,CancellationToken cancellation=default)
    {
        limits??=new();HistoryReader.Validate(h);
        if(limits.MaxStates<1 || limits.MaxStates>1_000_000 || limits.MaxMilliseconds<1 || limits.MaxMilliseconds>60_000)throw new ArgumentException("Invalid partition budget.");
        if(h.Model!=model.Name)throw new ArgumentException("History and model differ.");
        _=model.Initial(h.Initial);
        foreach(var operation in h.Operations)model.Validate(operation);
        var clock=System.Diagnostics.Stopwatch.StartNew();long states=0,transitions=0,hits=0;
        var orders=new List<Queue<string>>();
        foreach(var group in h.Operations.GroupBy(o=>o.Object,StringComparer.Ordinal))
        {
            var left=limits.MaxStates-states;var ms=limits.MaxMilliseconds-(int)clock.ElapsedMilliseconds;
            if(left<=0 || ms<=0 || cancellation.IsCancellationRequested)return Stop();
            var r=Checker.Check(h with{Operations=group.ToArray()},model,new(left,ms),cancellation);
            states+=r.States;transitions+=r.Transitions;hits+=r.CacheHits;
            if(r.Verdict!=Verdict.Linearizable)return r with{States=states,Transitions=transitions,CacheHits=hits,Reason="An object partition: "+r.Reason};
            orders.Add(new(r.Witness));
        }
        // Even an empty history must have a valid model/initial configuration.
        if(h.Operations.Length==0)return Checker.Check(h,model,limits,cancellation);
        var remaining=h.Operations.ToDictionary(o=>o.Id,StringComparer.Ordinal);var witness=new List<string>();
        while(remaining.Count>0)
        {
            var candidate=orders.FirstOrDefault(q=>q.Count>0 && !remaining.Values.Any(other=>other.End<remaining[q.Peek()].Start));
            if(candidate is null)return Stop();
            var id=candidate.Dequeue();witness.Add(id);remaining.Remove(id);
        }
        if(!WitnessVerifier.Verify(h,model,witness))return Stop();
        return new(Verdict.Linearizable,"All independent objects have legal histories; merged witness independently replayed.",witness.ToArray(),[],[],states,transitions,hits);
        CheckResult Stop()=>new(Verdict.Inconclusive,"Partition or witness merge did not complete within its contract.",[],[],[],states,transitions,hits);
    }
}
