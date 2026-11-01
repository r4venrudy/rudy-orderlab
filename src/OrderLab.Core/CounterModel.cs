using System.Globalization;
namespace Rudy.OrderLab;

public sealed class CounterModel : IModel
{
    public string Name=>"counter";
    internal static long Number(string text)
    {
        if(!long.TryParse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out var n) || n.ToString(CultureInfo.InvariantCulture)!=text)
            throw new ArgumentException("Expected canonical signed 64-bit decimal.");
        return n;
    }
    public string Initial(string[] values)
    { if(values.Length!=1) throw new ArgumentException("Counter needs one integer.");Number(values[0]);return values[0]; }
    public void Validate(Operation o)
    {
        if(o.Result is null || !((o.Kind=="read" && o.Args.Length==0)||(o.Kind=="add" && o.Args.Length==1))) throw new ArgumentException("Counter read()->integer or add(delta)->new integer.");
        Number(o.Result);if(o.Args.Length==1) Number(o.Args[0]);
    }
    public bool Apply(string state,Operation o,out string next,out string reason)
    {
        next=state;reason="observed counter result disagrees with sequential arithmetic";
        try
        {
            var n=Number(state);if(o.Kind=="add")n=checked(n+Number(o.Args[0]));
            if(n.ToString(CultureInfo.InvariantCulture)!=o.Result)return false;
            next=n.ToString(CultureInfo.InvariantCulture);return true;
        }
        catch(OverflowException) { reason="counter transition would overflow signed 64-bit arithmetic";return false; }
    }
}
