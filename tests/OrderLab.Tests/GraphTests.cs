using System.Numerics;
using Rudy.OrderLab;
internal static partial class Program
{
    private static void GraphTests()
    {
        var g=Precedence.Build(new(1,"register",["0"],[Op("a",0,3,"write",null,"1"),Op("b",1,2,"read","0"),Op("c",4,5,"read","1")]));
        Assert(g[0]==0 && g[1]==0 && g[2]==3,"overlap has no edge; later request depends on both");
        Assert(!Precedence.Enabled(g[2],BigInteger.One) && Precedence.Enabled(g[2],3),"predecessor mask");
    }
}
