namespace Rudy.OrderLab;

/// <summary>Independent replay: does not use the search engine or predecessor masks.</summary>
public static class WitnessVerifier
{
    public static bool Verify(History h,IModel model,IReadOnlyList<string> order)
    {
        HistoryReader.Validate(h);
        if(h.Model!=model.Name || order.Count!=h.Operations.Length) return false;
        foreach(var o in h.Operations) model.Validate(o);
        var state=model.Initial(h.Initial);
        var remaining=h.Operations.ToDictionary(o=>o.Id,StringComparer.Ordinal);
        foreach(var id in order)
        {
            if(!remaining.TryGetValue(id,out var o)) return false;
            if(remaining.Values.Any(other=>other.End<o.Start)) return false;
            if(!model.Apply(state,o,out var next,out _)) return false;
            state=next;remaining.Remove(id);
        }
        return remaining.Count==0;
    }
}
