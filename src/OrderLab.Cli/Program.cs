using System.Globalization;
using System.Security.Cryptography;
using Rudy.OrderLab;

internal static class Program
{
    private static string Usage=>"orderlab check HISTORY [--format text|json|html] [--output NEW_FILE] [--max-states 1..1000000] [--timeout-ms 1..60000] [--partition] [--include-values]\norderlab verify HISTORY --witness 1,2,3\nExit: 0=linearizable/valid witness, 1=rejected/invalid witness, 2=inconclusive or input/usage/I/O error.\n";
    public static int Main(string[] args)
    {
        try
        {
            if(args.Length==1 && args[0]=="--version"){Console.WriteLine("orderlab 0.1.0-preview");return 0;}
            if(args.Length==1 && args[0] is "--help" or "-h"){Console.Write(Usage);return 0;}
            if(args.Length<2 || args[0] is not ("check" or "verify" or "shrink"))throw new ArgumentException("Expected check, verify or shrink and a history path.");
            var options=new Dictionary<string,string>(StringComparer.Ordinal);
            for(int i=2;i<args.Length;i++)
            {
                var flag=args[i];if(!new[]{"--format","--output","--max-states","--timeout-ms","--max-checks","--partition","--include-values","--witness"}.Contains(flag))throw new ArgumentException("Unknown option.");
                var value=flag is "--partition" or "--include-values"?"true":++i<args.Length?args[i]:throw new ArgumentException("Missing option value.");
                if(!options.TryAdd(flag,value))throw new ArgumentException("Duplicate option.");
            }
            if(options.ContainsKey("--witness")!=(args[0]=="verify"))throw new ArgumentException("Only verify requires --witness.");
            using var source=new HistoryFile(args[1]);var h=HistoryReader.Read(source.Bytes);var model=Models.Get(h.Model);
            if(args[0]=="shrink")
            {
                if(!options.ContainsKey("--output") || options.Keys.Any(k=>!new[]{"--output","--max-checks","--max-states","--timeout-ms"}.Contains(k)))throw new ArgumentException("Shrink requires --output NEW_HISTORY and accepts only search/check budgets.");
                var reduced=Shrinker.Shrink(h,model,Parse(options.GetValueOrDefault("--max-checks","128")),new(Parse(options.GetValueOrDefault("--max-states","10000")),Parse(options.GetValueOrDefault("--timeout-ms","500"))));
                source.EnsureStable();ReportOutput.Write(HistoryWriter.Json(reduced.History),options["--output"]);
                Console.WriteLine($"Retained operations: {reduced.History.Operations.Length}; deletion checks: {reduced.Checks}; one-deletion-minimal: {reduced.OneMinimal}. {reduced.Reason}");return 1;
            }
            if(options.ContainsKey("--max-checks"))throw new ArgumentException("Only shrink accepts --max-checks.");
            if(args[0]=="verify")
            {
                var raw=options["--witness"];var nums=raw.Length==0?Array.Empty<int>():raw.Split(',').Select(Parse).ToArray();
                if(nums.Any(i=>i<1 || i>h.Operations.Length))throw new ArgumentException("Witness index out of range.");
                if(options.Keys.Any(k=>k!="--witness"))throw new ArgumentException("Verify accepts only --witness.");
                var ok=WitnessVerifier.Verify(h,model,nums.Select(i=>h.Operations[i-1].Id).ToArray());source.EnsureStable();Console.WriteLine(ok?"Witness verified.":"Witness rejected.");return ok?0:1;
            }
            var format=options.GetValueOrDefault("--format","text");if(format is not ("text" or "json" or "html"))throw new ArgumentException("Unknown report format.");
            var limits=new Limits(Parse(options.GetValueOrDefault("--max-states","100000")),Parse(options.GetValueOrDefault("--timeout-ms","10000")));
            var r=options.ContainsKey("--partition")?PartitionChecker.Check(h,model,limits):Checker.Check(h,model,limits);
            if(r.Verdict==Verdict.Linearizable && !WitnessVerifier.Verify(h,model,r.Witness))throw new InvalidOperationException("Internal witness verification failed.");
            source.EnsureStable();
            var report=Reports.Create(h,r,source.Sha256,options.ContainsKey("--include-values"));
            ReportOutput.Write(format switch{"json"=>Reports.Json(report),"html"=>Reports.Html(report),_=>Reports.Text(report)},options.GetValueOrDefault("--output"));
            return r.Verdict==Verdict.Linearizable?0:r.Verdict==Verdict.NotLinearizable?1:2;
        }
        catch(Exception e) when(e is ArgumentException or IOException or UnauthorizedAccessException or InvalidOperationException or OverflowException)
        { Console.Error.WriteLine("OrderLab: "+(e is IOException or UnauthorizedAccessException?"Unable to access the selected local file.":e.Message));return 2; }
    }
    private static int Parse(string s)
    { if(!int.TryParse(s,NumberStyles.None,CultureInfo.InvariantCulture,out var n))throw new ArgumentException("Expected an unsigned decimal option.");return n; }
}
