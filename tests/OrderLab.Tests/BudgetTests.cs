using Rudy.OrderLab;
internal static partial class Program
{
    private static void BudgetTests()
    {
        var h=new History(1,"register",["0"],[Op("w",0,1,"write",null,"1")]);
        Assert(Checker.Check(h,new RegisterModel(),new(1)).Verdict==Verdict.Inconclusive,"bounded search is never negative");
        using var c=new CancellationTokenSource();c.Cancel();
        Assert(Checker.Check(h,new RegisterModel(),cancellation:c.Token).Verdict==Verdict.Inconclusive,"canceled");
        Throws(()=>Checker.Check(h,new RegisterModel(),new(0)),"invalid state budget");
        Throws(()=>Checker.Check(h,new RegisterModel(),new(1,0)),"invalid time budget");
    }
}
