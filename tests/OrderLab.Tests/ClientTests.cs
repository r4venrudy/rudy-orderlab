using Rudy.OrderLab;
internal static partial class Program
{
    private static void ClientTests()
    {
        var a=Op("a",0,3,"read","0") with{Client="worker"};var b=Op("b",1,2,"read","0") with{Client="worker"};
        Throws(()=>HistoryReader.Validate(new(1,"register",["0"],[a,b])),"overlap on sequential client");
        HistoryReader.Validate(new(1,"register",["0"],[a,b with{Start=4,End=5}]));
        Assert(true,"sequential clients accepted across input order");
    }
}
