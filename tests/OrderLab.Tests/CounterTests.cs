using Rudy.OrderLab;
internal static partial class Program
{
    private static void CounterTests()
    {
        var m=new CounterModel();Assert(m.Apply("4",Op("a",0,1,"add","7","3"),out var n,out _) && n=="7","counter addition");
        Assert(!m.Apply("4",Op("a",0,1,"add","4","3"),out _,out _),"lost increment");
        Assert(!m.Apply(long.MaxValue.ToString(),Op("a",0,1,"add","0","1"),out _,out _),"overflow cannot wrap");
        foreach(var bad in new[]{"+1","01","-0","1.0","9223372036854775808"})Throws(()=>m.Initial([bad]),"canonical counter integer");
    }
}
