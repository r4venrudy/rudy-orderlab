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
        Assert(Reports.Text(r).Contains("Witness: 1") && Reports.Text(r).Contains("[0,1]"),"readable replay and intervals");
        Assert(Reports.Html(r).Contains("<svg") && Reports.Html(r).Contains("Content-Security-Policy") && !Reports.Html(r).Contains("<script"),"local static timeline");
        Assert(Reports.Html(r with{Reason="<img onerror=alert(1)>"}).Contains("&lt;img"),"HTML encoding");
        Assert(r.Initial[0].Sha256.Length==64 && r.Initial[0].Value is null,"digest-only default");
        Assert(Reports.Json(Reports.Create(h,Checker.Check(h,new RegisterModel()),new string('a',64),true)).Contains("secret"),"explicit value disclosure");
    }
}
