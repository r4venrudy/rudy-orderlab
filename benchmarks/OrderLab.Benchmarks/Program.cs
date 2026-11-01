using System.Diagnostics;
using System.Text.Json;
using Rudy.OrderLab;

var samples=new List<object>();
foreach(var width in new[]{4,8,12})
{
    // Concurrent identical writes have many equivalent legal prefixes. The
    // impossible read forces rejection, so a successful run explores failures.
    var ops=Enumerable.Range(0,width).Select(i=>new Operation("w"+i,"c"+i,"main",i,2L*width+1-i,"write",["0"],null)).ToList();
    ops.Add(new("impossible","reader","main",width,width+1,"read",[],"never-written"));
    var h=new History(1,"register",["0"],ops.ToArray());
    _=Checker.Check(h,new RegisterModel(),new(1_000_000,60_000));
    var before=GC.GetTotalAllocatedBytes(true);var timer=Stopwatch.StartNew();
    var r=Checker.Check(h,new RegisterModel(),new(1_000_000,60_000));timer.Stop();
    if(r.Verdict!=Verdict.NotLinearizable)throw new Exception("Benchmark did not finish a rejection.");
    samples.Add(new{width,operations=ops.Count,verdict=r.Verdict.ToString(),r.States,r.Transitions,r.CacheHits,elapsedMilliseconds=timer.Elapsed.TotalMilliseconds,allocatedBytes=GC.GetTotalAllocatedBytes(true)-before});
}
Console.WriteLine(JsonSerializer.Serialize(new{schema="orderlab.benchmark.v1",runtime=Environment.Version.ToString(),os=System.Runtime.InteropServices.RuntimeInformation.OSDescription,architecture=System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),samples},new JsonSerializerOptions{WriteIndented=true}));
