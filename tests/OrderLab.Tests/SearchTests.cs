using Rudy.OrderLab;
internal static partial class Program
{
    private static void SearchTests()
    {
        var m=new RegisterModel();
        var good=new History(1,"register",["0"],[Op("w",0,3,"write",null,"1"),Op("r",1,2,"read","0")]);
        var r=Checker.Check(good,m);Assert(r.Verdict==Verdict.Linearizable && r.Witness.SequenceEqual(new[]{"r","w"}),"backtrack concurrent read before write");
        var bad=good with{Operations=[Op("w",0,1,"write",null,"1"),Op("r",2,3,"read","0")]};
        var rejected=Checker.Check(bad,m);
        Assert(rejected.Verdict==Verdict.NotLinearizable,"stale nonoverlap read");
        Assert(rejected.Frontier.SequenceEqual(new[]{"w"}) && rejected.Rejections.Single().OperationId=="r","explain deepest legal prefix");
        Assert(Checker.Check(good with{Operations=[]},m).Verdict==Verdict.Linearizable,"empty is linearizable");
    }
}
