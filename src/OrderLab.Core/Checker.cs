using System.Numerics;
namespace Rudy.OrderLab;

public static class Checker
{
    public static CheckResult Check(History history, IModel model, Limits? limits=null, CancellationToken cancellation=default)
    {
        limits??=new();
        if(limits.MaxStates<1 || limits.MaxStates>1_000_000 || limits.MaxMilliseconds<1 || limits.MaxMilliseconds>60_000)
            throw new ArgumentException("Budget outside supported range.");
        var clock=System.Diagnostics.Stopwatch.StartNew();
        HistoryReader.Validate(history);
        if(history.Model!=model.Name) throw new ArgumentException("History and model differ.");
        if(history.Operations.Select(o=>o.Object).Distinct(StringComparer.Ordinal).Count()>1) throw new ArgumentException("Check one object at a time.");
        var initial=model.Initial(history.Initial);
        foreach(var o in history.Operations) model.Validate(o);
        var predecessors=Precedence.Build(history);
        var all=(BigInteger.One<<history.Operations.Length)-1;
        var path=new List<string>();string[] witness=[];
        long states=0,transitions=0,cacheHits=0,cacheChars=0;bool exhausted=false;
        var failed=new HashSet<string>(StringComparer.Ordinal);
        bool Search(BigInteger done,string state)
        {
            if(cancellation.IsCancellationRequested || clock.ElapsedMilliseconds>=limits.MaxMilliseconds || states>=limits.MaxStates)
            { exhausted=true;return false; }
            states++;
            if(done==all) { witness=path.ToArray();return true; }
            var key=done.ToString("X",System.Globalization.CultureInfo.InvariantCulture)+":"+state;
            if(failed.Contains(key)) { cacheHits++;return false; }
            for(int i=0;i<history.Operations.Length;i++)
            {
                var bit=BigInteger.One<<i;
                if((done&bit)!=0 || !Precedence.Enabled(predecessors[i],done)) continue;
                transitions++;
                var o=history.Operations[i];
                if(!model.Apply(state,o,out var next,out _)) continue;
                path.Add(o.Id);
                if(Search(done|bit,next)) return true;
                path.RemoveAt(path.Count-1);
                if(exhausted) return false;
            }
            // Exact state keys, no hash-only equality; stop caching at 32 MiB of UTF-16.
            if(cacheChars+key.Length<=16*1024*1024 && failed.Add(key)) cacheChars+=key.Length;
            return false;
        }
        var accepted=Search(0,initial);
        return new(accepted?Verdict.Linearizable:exhausted?Verdict.Inconclusive:Verdict.NotLinearizable,
            accepted?"A legal sequential witness preserves every real-time edge.":exhausted?"Search stopped at a state/time budget or cancellation; no negative conclusion.":"Every real-time-respecting candidate contradicts the model.",
            witness,[],[],states,transitions,cacheHits);
    }
}
