using Rudy.OrderLab;
internal static partial class Program
{
    private static void MapTests()
    {
        var m=new MapModel();var s=m.Initial(["key","value"]);
        Assert(m.Apply(s,Op("g",0,1,"get","value","key"),out _,out _),"map lookup");
        Assert(m.Apply(s,Op("g",0,1,"get",null,"absent"),out _,out _),"map absence");
        Assert(m.Apply(s,Op("d",0,1,"delete","true","key"),out var n,out _) && m.Apply(n,Op("s",2,3,"size","0"),out _,out _),"map deletion");
        Throws(()=>m.Initial(["k","v","k","v2"]),"ambiguous initial map");
    }
}
