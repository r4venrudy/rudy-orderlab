using System.Text;
using System.Globalization;
namespace Rudy.OrderLab;
public static partial class Reports
{
    public static string Text(Report r)
    {
        var b=new StringBuilder();b.AppendLine("RUDY ORDERLAB / "+r.Verdict);b.AppendLine(r.Reason);
        b.AppendLine("Model: "+r.Model+" | Operations: "+r.Operations.Length.ToString(CultureInfo.InvariantCulture));
        b.AppendLine("Input SHA-256: "+r.InputSha256);
        b.AppendLine($"States: {r.Stats.States.ToString(CultureInfo.InvariantCulture)} | Transitions: {r.Stats.Transitions.ToString(CultureInfo.InvariantCulture)} | Cache hits: {r.Stats.CacheHits.ToString(CultureInfo.InvariantCulture)}");
        if(r.Witness.Length>0)b.AppendLine("Witness: "+string.Join(" -> ",r.Witness));
        if(r.Frontier.Length>0)b.AppendLine("One explored legal prefix: "+string.Join(" -> ",r.Frontier));
        foreach(var rejection in r.Rejections)b.AppendLine("Operation "+rejection.Operation+": "+rejection.Reason);
        foreach(var o in r.Operations)b.AppendLine(FormattableString.Invariant($"#{o.Number} / client {o.Client} / object {o.Object} / [{o.Start},{o.End}] / {o.Kind}"));
        return b.ToString();
    }
}
