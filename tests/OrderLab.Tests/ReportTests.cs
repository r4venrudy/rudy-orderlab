using System.Text.Json;
using Rudy.OrderLab;
internal static partial class Program
{
    private static void ReportTests()
    {
        var h=new History(1,"register",["secret"],[Op("private-id",0,1,"read","secret")]);
        var r=Reports.Create(h,Checker.Check(h,new RegisterModel()),new string('a',64));
        using var doc=JsonDocument.Parse(Reports.Json(r));
        Assert(doc.RootElement.GetProperty("schema").GetString()=="orderlab.report.v1","versioned report");
        Assert(!Reports.Json(r).Contains("secret") && !Reports.Json(r).Contains("private-id"),"opaque operation labels");
    }
}
