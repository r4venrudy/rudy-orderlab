using System.Text;
using Rudy.OrderLab;
internal static partial class Program
{
    private static void ReaderTests()
    {
        Assert(HistoryReader.Read(Encoding.UTF8.GetBytes("{\"schema\":1,\"model\":\"register\",\"initial\":[\"0\"],\"operations\":[]}")).Operations.Length == 0,"empty reader");
        foreach(var bad in new[]{"{}","[]","{\"schema\":1,\"schema\":1}","{\"schema\":2,\"model\":\"register\",\"initial\":[],\"operations\":[]}"})
            Throws(()=>HistoryReader.Read(Encoding.UTF8.GetBytes(bad)),"bad JSON/schema");
        Throws(()=>HistoryReader.Validate(new(1,"register",["0"],[Op("a",1,2,"read","0"),Op("a",3,4,"read","0")])),"duplicate id");
        Throws(()=>HistoryReader.Validate(new(1,"register",["0"],[Op("a",1,2,"read","0"),Op("b",2,3,"read","0")])),"ambiguous ticks");
    }
}
