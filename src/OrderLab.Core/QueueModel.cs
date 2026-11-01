using System.Text.Json;
namespace Rudy.OrderLab;
public class QueueModel : IModel
{
    public virtual string Name=>"queue";
    protected virtual string Insert=>"enqueue";
    protected virtual string Remove=>"dequeue";
    protected virtual int Head(int count)=>0;
    public string Initial(string[] values)=>JsonSerializer.Serialize(values);
    public void Validate(Operation o)
    {
        if(o.Kind==Insert && o.Args.Length==1 && o.Result is null)return;
        if((o.Kind==Remove || o.Kind=="peek") && o.Args.Length==0)return;
        throw new ArgumentException("Expected insertion(value)->null, removal()->value/null or peek()->value/null.");
    }
    public bool Apply(string state,Operation o,out string next,out string reason)
    {
        var values=JsonSerializer.Deserialize<List<string>>(state)!;
        reason="sequence head or empty result disagrees with the observation";
        if(o.Kind==Insert) { values.Add(o.Args[0]);next=JsonSerializer.Serialize(values);return true; }
        var value=values.Count==0?null:values[Head(values.Count)];
        if(o.Kind==Remove && values.Count>0)values.RemoveAt(Head(values.Count));
        next=JsonSerializer.Serialize(values);return value==o.Result;
    }
}
