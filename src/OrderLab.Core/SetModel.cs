using System.Text.Json;
namespace Rudy.OrderLab;
public sealed class SetModel : IModel
{
    public string Name=>"set";
    public string Initial(string[] values)
    { if(values.Distinct(StringComparer.Ordinal).Count()!=values.Length)throw new ArgumentException("Duplicate initial set element.");return Encode(values); }
    private static string Encode(IEnumerable<string> values)=>JsonSerializer.Serialize(values.Order(StringComparer.Ordinal).ToArray());
    public void Validate(Operation o)
    {
        if(o.Kind=="size" && o.Args.Length==0 && o.Result is not null) { if(CounterModel.Number(o.Result)<0)throw new ArgumentException("Negative size.");return; }
        if(o.Args.Length!=1 || !new[]{"add","remove","contains"}.Contains(o.Kind) || o.Result is not ("true" or "false"))throw new ArgumentException("Set operation requires one element and a boolean result.");
    }
    public bool Apply(string state,Operation o,out string next,out string reason)
    {
        var set=new HashSet<string>(JsonSerializer.Deserialize<string[]>(state)!,StringComparer.Ordinal);
        reason="set membership, mutation or size disagrees with the observation";
        var expected=o.Kind switch {"add"=>set.Add(o.Args[0])?"true":"false","remove"=>set.Remove(o.Args[0])?"true":"false","contains"=>set.Contains(o.Args[0])?"true":"false",_=>set.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)};
        next=Encode(set);return expected==o.Result;
    }
}
