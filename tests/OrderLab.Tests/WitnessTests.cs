using Rudy.OrderLab;
internal static partial class Program
{
    private static void WitnessTests()
    {
        var h=new History(1,"register",["0"],[Op("w",0,3,"write",null,"1"),Op("r",1,2,"read","0")]);
        Assert(WitnessVerifier.Verify(h,new RegisterModel(),["r","w"]),"independent replay");
        foreach(var order in new string[][]{["w","r"],["r","r"],["w"],["unknown","r"]})
            Assert(!WitnessVerifier.Verify(h,new RegisterModel(),order),"reject forged witness");
    }
}
