using Rudy.OrderLab;
internal static partial class Program
{
    private static void CacheTests()
    {
        var h=new History(1,"register",["0"],[Op("a",0,5,"write",null,"0"),Op("b",1,4,"write",null,"0"),Op("r",2,3,"read","impossible")]);
        var r=Checker.Check(h,new RegisterModel());
        Assert(r.Verdict==Verdict.NotLinearizable && r.CacheHits>0,"equivalent failed states are reused");
    }
}
