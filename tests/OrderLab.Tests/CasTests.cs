using Rudy.OrderLab;
internal static partial class Program
{
    private static void CasTests()
    {
        var h=new History(1,"cas-register",["0"],[Op("a",0,3,"cas","true","0","1"),Op("b",1,2,"cas","true","0","2")]);
        Assert(Checker.Check(h,Models.Get(h.Model)).Verdict==Verdict.NotLinearizable,"double successful CAS cannot both observe zero");
        var m=new CasRegisterModel();Assert(m.Apply("1",Op("a",0,1,"cas","false","0","2"),out var n,out _) && n=="1","failed CAS unchanged");
        Throws(()=>Models.Get("plugin-from-input"),"no dynamic model loading");
    }
}
