using System.Text.Json;
namespace Rudy.OrderLab;
public sealed class MapModel : IModel
{
    public string Name=>"map";
    private static string Encode(SortedDictionary<string,string> state)=>JsonSerializer.Serialize(state);
    public string Initial(string[] values)
    {
        if(values.Length%2!=0)throw new ArgumentException("Map initial values are alternating key/value pairs.");
        var map=new SortedDictionary<string,string>(StringComparer.Ordinal);
        for(int i=0;i<values.Length;i+=2)if(!map.TryAdd(values[i],values[i+1]))throw new ArgumentException("Duplicate map key.");
        return Encode(map);
    }
    public void Validate(Operation o)
    {
        if(o.Kind=="put" && o.Args.Length==2 && o.Result is null)return;
        if(o.Kind=="get" && o.Args.Length==1)return;
        if(o.Kind=="delete" && o.Args.Length==1 && o.Result is "true" or "false")return;
        if(o.Kind=="size" && o.Args.Length==0 && o.Result is not null && CounterModel.Number(o.Result)>=0)return;
        throw new ArgumentException("Map supports put(key,value)->null, get(key)->value/null, delete(key)->boolean and size()->integer.");
    }
    public bool Apply(string state,Operation o,out string next,out string reason)
    {
        var map=new SortedDictionary<string,string>(JsonSerializer.Deserialize<Dictionary<string,string>>(state)!,StringComparer.Ordinal);
        reason="map result contradicts the current key/value state";string? expected=null;
        switch(o.Kind)
        {
            case "put":map[o.Args[0]]=o.Args[1];break;
            case "get":expected=map.GetValueOrDefault(o.Args[0]);break;
            case "delete":expected=map.Remove(o.Args[0])?"true":"false";break;
            case "size":expected=map.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);break;
        }
        next=Encode(map);return expected==o.Result;
    }
}
