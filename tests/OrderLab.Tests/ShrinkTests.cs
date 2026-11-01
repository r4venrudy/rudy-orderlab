using Rudy.OrderLab;
internal static partial class Program
{
    private static void ShrinkTests()
    {
        var h=new History(1,"register",["0"],[Op("w",0,1,"write",null,"1"),Op("extra",2,3,"write",null,"0") with{Object="B"},Op("r",4,5,"read","0")]);
        var r=Shrinker.Shrink(h,new RegisterModel());
        Assert(r.OneMinimal && r.History.Operations.Length==2,"remove irrelevant operation");
        Assert(Checker.Check(r.History,new RegisterModel()).Verdict==Verdict.NotLinearizable,"shrunk rejection rechecked");
        Assert(!Shrinker.Shrink(h,new RegisterModel(),1).OneMinimal,"budget cannot claim minimal");
        Assert(HistoryReader.Read(System.Text.Encoding.UTF8.GetBytes(HistoryWriter.Json(r.History))).Operations.Length==2,"shrunk history roundtrip");
    }
}
