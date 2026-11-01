using System.Text.Json;
using System.Text.Json.Serialization;
namespace Rudy.OrderLab;

public sealed record OperationView(int Number,int Client,int Object,long Start,long End,string Kind);
public sealed record RejectionView(int Operation,string Reason);
public sealed record SearchStats(long States,long Transitions,long CacheHits);
public sealed record Report(string Schema,string ToolVersion,string InputSha256,string Model,string Verdict,string Reason,
    OperationView[] Operations,int[] Witness,int[] Frontier,RejectionView[] Rejections,SearchStats Stats);
public static partial class Reports
{
    private static readonly JsonSerializerOptions Options=new(){PropertyNamingPolicy=JsonNamingPolicy.CamelCase,WriteIndented=true,DefaultIgnoreCondition=JsonIgnoreCondition.WhenWritingNull};
    public static Report Create(History h,CheckResult r,string inputSha256)
    {
        if(inputSha256.Length!=64 || inputSha256.Any(c=>!char.IsAsciiHexDigit(c)))throw new ArgumentException("Expected SHA-256 identity.");
        var numbers=h.Operations.Select((o,i)=>(o.Id,Number:i+1)).ToDictionary(x=>x.Id,x=>x.Number,StringComparer.Ordinal);
        var clients=h.Operations.Select(o=>o.Client).Distinct(StringComparer.Ordinal).Select((v,i)=>(v,i)).ToDictionary(x=>x.v,x=>x.i+1,StringComparer.Ordinal);
        var objects=h.Operations.Select(o=>o.Object).Distinct(StringComparer.Ordinal).Select((v,i)=>(v,i)).ToDictionary(x=>x.v,x=>x.i+1,StringComparer.Ordinal);
        return new("orderlab.report.v1","0.1.0-preview",inputSha256.ToLowerInvariant(),h.Model,r.Verdict.ToString(),r.Reason,
            h.Operations.Select((o,i)=>new OperationView(i+1,clients[o.Client],objects[o.Object],o.Start,o.End,o.Kind)).ToArray(),
            r.Witness.Select(id=>numbers[id]).ToArray(),r.Frontier.Select(id=>numbers[id]).ToArray(),r.Rejections.Select(x=>new RejectionView(numbers[x.OperationId],x.Reason)).ToArray(),new(r.States,r.Transitions,r.CacheHits));
    }
    public static string Json(Report report)=>JsonSerializer.Serialize(report,Options)+"\n";
}
