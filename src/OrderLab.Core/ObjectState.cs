using System.Text.Json;
namespace Rudy.OrderLab;
internal static class ObjectState
{
    internal static string Initial(History h,IModel model)
    {
        var initial=model.Initial(h.Initial);
        var objects=new SortedDictionary<string,string>(StringComparer.Ordinal);
        foreach(var o in h.Operations)objects[o.Object]=initial;
        return JsonSerializer.Serialize(objects);
    }
    internal static bool Apply(string state,Operation operation,IModel model,out string next,out string reason)
    {
        var objects=new SortedDictionary<string,string>(JsonSerializer.Deserialize<Dictionary<string,string>>(state)!,StringComparer.Ordinal);
        if(!model.Apply(objects[operation.Object],operation,out var value,out reason)){next=state;return false;}
        objects[operation.Object]=value;next=JsonSerializer.Serialize(objects);return true;
    }
}
