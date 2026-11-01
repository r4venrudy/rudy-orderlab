using System.Net;
using System.Text;
using System.Globalization;
namespace Rudy.OrderLab;
public static partial class Reports
{
    private static string E(string value)=>WebUtility.HtmlEncode(value);
    public static string Html(Report r)
    {
        var b=new StringBuilder("<!doctype html><html lang=\"en\"><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src 'unsafe-inline'; img-src data:; base-uri 'none'; form-action 'none'\"><title>Rudy OrderLab investigation</title><style>:root{color-scheme:dark}*{box-sizing:border-box}body{margin:0;background:#090f17;color:#edf6ff;font:15px/1.6 system-ui,sans-serif}main{max-width:1220px;margin:40px auto;padding:24px}header{background:#101e2b;border-left:4px solid #55e0b6;padding:28px;border-radius:8px}small,h2{color:#55e0b6}h1{font-size:44px;line-height:1.15;margin:16px 0}p{color:#a5b8cd}.cards{display:flex;gap:16px;flex-wrap:wrap;margin:24px 0}.card{background:#101b28;border:1px solid #293e52;border-radius:10px;padding:20px;flex:1;min-width:160px}.value{font-size:30px;color:#55e0b6}.scroll{overflow:auto}svg{min-width:900px;width:100%;background:#101b28;border-radius:10px}text{fill:#b5c8dc;font:12px monospace}rect.bar{fill:#55e0b6;opacity:.8}line{stroke:#293e52}table{border-collapse:collapse;width:100%}td,th{padding:12px;border-bottom:1px solid #293e52;text-align:left}code{overflow-wrap:anywhere}details{margin:24px 0;border:1px solid #293e52;padding:20px;border-radius:10px}pre{white-space:pre-wrap;overflow-wrap:anywhere;font:12px/1.6 monospace}footer{border-top:1px solid #293e52;padding-top:20px;color:#a5b8cd}</style><main><header><small>RUDY SYSTEMS / CONCURRENCY INVESTIGATION</small><h1>OrderLab</h1><p>");
        b.Append(E(r.Verdict)).Append(" · ").Append(E(r.Model)).Append("</p><p>").Append(E(r.Reason)).Append("</p></header><div class=\"cards\">");
        foreach(var item in new[]{("Operations",(long)r.Operations.Length),("States",r.Stats.States),("Transitions",r.Stats.Transitions),("Cache hits",r.Stats.CacheHits)})
            b.Append("<div class=\"card\">").Append(item.Item1).Append("<div class=\"value\">").Append(item.Item2.ToString(CultureInfo.InvariantCulture)).Append("</div></div>");
        b.Append("</div><h2>Observed intervals</h2><p>Each bar spans invocation to response on one globally ordered logical clock. Overlap permits reordering; it does not prove simultaneous execution.</p><div class=\"scroll\">");
        var height=64+36*r.Operations.Length;var max=r.Operations.Length==0?1:r.Operations.Max(o=>o.End);
        b.Append(FormattableString.Invariant($"<svg role=\"img\" aria-label=\"Operation invocation and response intervals\" viewBox=\"0 0 1100 {height}\">"));
        b.Append("<text x=\"20\" y=\"26\">OP / CLIENT / OBJECT</text><text x=\"230\" y=\"26\">GLOBAL LOGICAL TICKS</text>");
        for(int i=0;i<r.Operations.Length;i++)
        {
            var o=r.Operations[i];var y=50+36*i;var x=230m+820m*o.Start/max;var width=Math.Max(2m,820m*((decimal)o.End-o.Start)/max);
            b.Append(FormattableString.Invariant($"<text x=\"20\" y=\"{y+16}\">#{o.Number} / C{o.Client} / O{o.Object}</text><rect class=\"bar\" x=\"{x:F3}\" y=\"{y}\" width=\"{width:F3}\" height=\"22\"><title>"));
            b.Append(E(FormattableString.Invariant($"{o.Kind} [{o.Start},{o.End}]"))).Append("</title></rect>");
        }
        b.Append("</svg></div><h2>Replay and conflicts</h2><p>Witness: ").Append(E(string.Join(" → ",r.Witness))).Append("</p><p>One explored legal prefix: ").Append(E(string.Join(" → ",r.Frontier))).Append("</p>");
        foreach(var x in r.Rejections)b.Append("<p>Operation #").Append(x.Operation).Append(": ").Append(E(x.Reason)).Append("</p>");
        b.Append("<details><summary>Structured evidence</summary><pre>").Append(E(Json(r))).Append("</pre></details><footer>Developer preview. Conclusions apply only to this complete history, model and clock contract. A rejected history does not identify a unique root cause; a passing history is not a proof of the implementation. Input SHA-256: <code>").Append(E(r.InputSha256)).Append("</code></footer></main></html>\n");
        return b.ToString();
    }
}
