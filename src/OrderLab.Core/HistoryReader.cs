using System.Text.Json;

namespace Rudy.OrderLab;

public static class HistoryReader
{
    public const int MaxBytes = 8 * 1024 * 1024;
    public const int MaxOperations = 256;
    public static History Read(ReadOnlyMemory<byte> utf8)
    {
        if (utf8.Length > MaxBytes) throw new ArgumentException("Input exceeds 8 MiB.");
        try
        {
            using var doc = JsonDocument.Parse(utf8, new JsonDocumentOptions { MaxDepth = 16 });
            Unique(doc.RootElement);
            var root = doc.RootElement;
            Shape(root, "schema", "model", "initial", "operations");
            if (root.GetProperty("schema").GetInt32() != 1) throw new ArgumentException("Unsupported history schema.");
            var model = Text(root.GetProperty("model"));
            var initial = Strings(root.GetProperty("initial"));
            var ops = root.GetProperty("operations").EnumerateArray().Select(o =>
            {
                Shape(o, "id", "client", "object", "start", "end", "kind", "args", "result");
                return new Operation(Text(o.GetProperty("id")), Text(o.GetProperty("client")), Text(o.GetProperty("object")),
                    o.GetProperty("start").GetInt64(), o.GetProperty("end").GetInt64(), Text(o.GetProperty("kind")),
                    Strings(o.GetProperty("args")), o.GetProperty("result").ValueKind == JsonValueKind.Null ? null : Text(o.GetProperty("result")));
            }).ToArray();
            var h = new History(1, model, initial, ops);
            Validate(h); return h;
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or FormatException or KeyNotFoundException or OverflowException)
        { throw new ArgumentException("Malformed history: expected the documented complete-operation schema."); }
    }
    public static void Validate(History h)
    {
        if (h.Schema != 1 || h.Operations.Length > MaxOperations || h.Initial.Length > 256) throw new ArgumentException("History schema or count limit.");
        ValidateText(h.Model);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var ticks = new HashSet<long>();
        foreach (var value in h.Initial) ValidateText(value);
        foreach (var o in h.Operations)
        {
            ValidateText(o.Id); ValidateText(o.Client); ValidateText(o.Object); ValidateText(o.Kind);
            if (o.Id.Length == 0 || o.Client.Length == 0 || o.Object.Length == 0 || !ids.Add(o.Id)) throw new ArgumentException("Operation identities must be nonempty and unique.");
            if (o.Start < 0 || o.End <= o.Start || !ticks.Add(o.Start) || !ticks.Add(o.End)) throw new ArgumentException("Ticks must be nonnegative, unique and globally ordered; start < end.");
            if (o.Args.Length > 4) throw new ArgumentException("At most four operation arguments.");
            foreach (var a in o.Args) ValidateText(a);
            if (o.Result is not null) ValidateText(o.Result);
        }
        foreach(var client in h.Operations.GroupBy(o=>o.Client,StringComparer.Ordinal))
        {
            long last=-1;
            foreach(var o in client.OrderBy(o=>o.Start))
            { if(o.Start<=last)throw new ArgumentException("A sequential client cannot have overlapping operations; use separate client IDs for concurrent workers.");last=o.End; }
        }
    }
    private static void ValidateText(string value)
    {
        if (value is null || value.Length > 1024 || value.Any(char.IsControl)) throw new ArgumentException("Strings must be bounded printable Unicode.");
        try { _ = new System.Text.UTF8Encoding(false, true).GetByteCount(value); }
        catch (System.Text.EncoderFallbackException) { throw new ArgumentException("Invalid Unicode string."); }
    }
    private static string Text(JsonElement e) { var s = e.GetString() ?? throw new ArgumentException("Expected string."); ValidateText(s); return s; }
    private static string[] Strings(JsonElement e)
    { if (e.GetArrayLength() > 256) throw new ArgumentException("Array exceeds limit."); return e.EnumerateArray().Select(Text).ToArray(); }
    private static void Shape(JsonElement e, params string[] properties)
    {
        if (e.ValueKind != JsonValueKind.Object) throw new ArgumentException("Expected object.");
        var actual = e.EnumerateObject().Select(p => p.Name).ToArray();
        if (actual.Length != properties.Length || actual.Except(properties, StringComparer.Ordinal).Any()) throw new ArgumentException("Missing or unknown history property.");
    }
    private static void Unique(JsonElement e)
    {
        if (e.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in e.EnumerateObject()) { if (!names.Add(p.Name)) throw new ArgumentException("Duplicate JSON property."); Unique(p.Value); }
        }
        else if (e.ValueKind == JsonValueKind.Array) foreach (var item in e.EnumerateArray()) Unique(item);
    }
}
