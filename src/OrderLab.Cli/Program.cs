using System.Globalization;
using System.Security.Cryptography;
using Rudy.OrderLab;

internal static class Program
{
    private static string Usage=>"orderlab check HISTORY [--format text|json|html] [--max-states 1..1000000] [--timeout-ms 1..60000] [--partition] [--include-values]\norderlab verify HISTORY --witness 1,2,3\nExit: 0=linearizable/valid witness, 1=rejected/invalid witness, 2=inconclusive or input/usage/I/O error.\n";
    public static int Main(string[] args)
    {
        try
        {
            if(args.Length==1 && args[0]=="--version"){Console.WriteLine("orderlab 0.1.0-preview");return 0;}
            if(args.Length==1 && args[0] is "--help" or "-h"){Console.Write(Usage);return 0;}
            if(args.Length<2 || args[0] is not ("check" or "verify"))throw new ArgumentException("Expected a command and history path.");
            var options=new Dictionary<string,string>(StringComparer.Ordinal);
            for(int i=2;i<args.Length;i++)
            {
                var flag=args[i];if(!new[]{"--format","--max-states","--timeout-ms","--partition","--include-values","--witness"}.Contains(flag))throw new ArgumentException("Unknown option.");
                var value=flag is "--partition" or "--include-values"?"true":++i<args.Length?args[i]:throw new ArgumentException("Missing option value.");
                if(!options.TryAdd(flag,value))throw new ArgumentException("Duplicate option.");
            }
            if(options.ContainsKey("--witness")!=(args[0]=="verify"))throw new ArgumentException("Only verify requires --witness.");
            var bytes=File.ReadAllBytes(args[1]);var h=HistoryReader.Read(bytes);var model=Models.Get(h.Model);
            if(args[0]=="verify")
            {
                var raw=options["--witness"];var nums=raw.Length==0?Array.Empty<int>():raw.Split(',').Select(Parse).ToArray();
                if(nums.Any(i=>i<1 || i>h.Operations.Length))throw new ArgumentException("Witness index out of range.");
                var ok=WitnessVerifier.Verify(h,model,nums.Select(i=>h.Operations[i-1].Id).ToArray());Console.WriteLine(ok?"Witness verified.":"Witness rejected.");return ok?0:1;
            }
            var format=options.GetValueOrDefault("--format","text");if(format is not ("text" or "json" or "html"))throw new ArgumentException("Unknown report format.");
            var limits=new Limits(Parse(options.GetValueOrDefault("--max-states","100000")),Parse(options.GetValueOrDefault("--timeout-ms","10000")));
            var r=options.ContainsKey("--partition")?PartitionChecker.Check(h,model,limits):Checker.Check(h,model,limits);
            if(r.Verdict==Verdict.Linearizable && !WitnessVerifier.Verify(h,model,r.Witness))throw new InvalidOperationException("Internal witness verification failed.");
            var report=Reports.Create(h,r,Convert.ToHexStringLower(SHA256.HashData(bytes)),options.ContainsKey("--include-values"));
            Console.Write(format switch{"json"=>Reports.Json(report),"html"=>Reports.Html(report),_=>Reports.Text(report)});
            return r.Verdict==Verdict.Linearizable?0:r.Verdict==Verdict.NotLinearizable?1:2;
        }
        catch(Exception e) when(e is ArgumentException or IOException or UnauthorizedAccessException or InvalidOperationException or OverflowException)
        { Console.Error.WriteLine("OrderLab: "+(e is IOException or UnauthorizedAccessException?"Unable to access the selected local file.":e.Message));return 2; }
    }
    private static int Parse(string s)
    { if(!int.TryParse(s,NumberStyles.None,CultureInfo.InvariantCulture,out var n))throw new ArgumentException("Expected an unsigned decimal option.");return n; }
}
