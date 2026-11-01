using Rudy.OrderLab;
internal static partial class Program
{
    private static void StackTests()
    {
        var m=new StackModel();var s=m.Initial(["bottom","top"]);
        Assert(m.Apply(s,Op("p",0,1,"pop","top"),out var n,out _) && m.Apply(n,Op("k",2,3,"peek","bottom"),out _,out _),"LIFO");
        Assert(!m.Apply(s,Op("p",0,1,"pop","bottom"),out _,out _),"LIFO inversion");
        Throws(()=>m.Validate(Op("p",0,1,"dequeue","top")),"queue verbs cannot enter stack model");
    }
}
