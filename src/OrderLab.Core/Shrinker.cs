namespace Rudy.OrderLab;
public sealed record ShrinkResult(History History,bool OneMinimal,int Checks,string Reason);
public static class Shrinker
{
    public static ShrinkResult Shrink(History h,IModel model,int maxChecks=128,Limits? perCheck=null,CancellationToken cancellation=default)
    {
        if(maxChecks<1 || maxChecks>512)throw new ArgumentException("Shrink check budget must be 1..512.");
        perCheck??=new(10_000,500);
        var checks=1;var clock=System.Diagnostics.Stopwatch.StartNew();
        if(Checker.Check(h,model,perCheck,cancellation).Verdict!=Verdict.NotLinearizable)throw new ArgumentException("Shrinking requires an exhaustively rejected history.");
        var current=h;bool changed=true;bool conclusive=true;
        while(changed)
        {
            changed=false;conclusive=true;
            for(int i=0;i<current.Operations.Length;i++)
            {
                if(checks>=maxChecks || clock.ElapsedMilliseconds>=10_000 || cancellation.IsCancellationRequested)return new(current,false,checks,"Stopped; retained history is still rejected, minimality unproved.");
                var candidate=current with{Operations=current.Operations.Where((_,j)=>i!=j).ToArray()};
                var r=Checker.Check(candidate,model,perCheck,cancellation);checks++;
                if(r.Verdict==Verdict.Inconclusive)conclusive=false;
                if(r.Verdict!=Verdict.NotLinearizable)continue;
                current=candidate;changed=true;break;
            }
        }
        return new(current,conclusive,checks,conclusive?"No single operation can be deleted while retaining a rejected history; not a globally smallest proof.":"Retained rejection is valid; some deletion checks were inconclusive.");
    }
}
