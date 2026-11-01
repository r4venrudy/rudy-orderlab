using Rudy.OrderLab;
internal static partial class Program
{
    private static void SetTests()
    {
        var m=new SetModel();var s=m.Initial(["b","a"]);
        Assert(m.Apply(s,Op("a",0,1,"add","false","a"),out var n,out _) && n==s,"duplicate add");
        Assert(m.Apply(s,Op("r",0,1,"remove","true","a"),out n,out _) && m.Apply(n,Op("s",2,3,"size","1"),out _,out _),"remove and size");
        Throws(()=>m.Initial(["a","a"]),"initial set uniqueness");
    }
}
