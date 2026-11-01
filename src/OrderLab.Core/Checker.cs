using System.Numerics;
namespace Rudy.OrderLab;

public static class Checker
{
    public static CheckResult Check(History history, IModel model)
    {
        HistoryReader.Validate(history);
        if(history.Model!=model.Name) throw new ArgumentException("History and model differ.");
        if(history.Operations.Select(o=>o.Object).Distinct(StringComparer.Ordinal).Count()>1) throw new ArgumentException("Check one object at a time.");
        var initial=model.Initial(history.Initial);
        foreach(var o in history.Operations) model.Validate(o);
        var predecessors=Precedence.Build(history);
        var all=(BigInteger.One<<history.Operations.Length)-1;
        var path=new List<string>();string[] witness=[];
        long states=0,transitions=0;bool exhausted=false;
        bool Search(BigInteger done,string state)
        {
            if(++states>1_000_000) { exhausted=true;return false; }
            if(done==all) { witness=path.ToArray();return true; }
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
            return false;
        }
        var accepted=Search(0,initial);
        return new(accepted?Verdict.Linearizable:exhausted?Verdict.Inconclusive:Verdict.NotLinearizable,
            accepted?"A legal sequential witness preserves every real-time edge.":exhausted?"State budget exhausted.":"Every real-time-respecting candidate contradicts the model.",
            witness,[],[],states,transitions,0);
    }
}
