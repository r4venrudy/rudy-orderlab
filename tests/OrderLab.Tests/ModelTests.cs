using Rudy.OrderLab;
internal static partial class Program
{
    private static void ModelTests()
    {
        var m=new RegisterModel();Assert(m.Initial(["zero"])=="zero","register initial");
        Assert(m.Apply("0",Op("w",0,1,"write",null,"1"),out var n,out _) && n=="1","register write");
        Assert(!m.Apply("1",Op("r",0,1,"read","0"),out _,out _),"stale read rejected");
        Throws(()=>m.Validate(Op("w",0,1,"write","ack","1")),"unexpected ack");
    }
}
