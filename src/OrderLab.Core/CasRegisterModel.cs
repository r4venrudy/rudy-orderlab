namespace Rudy.OrderLab;
public sealed class CasRegisterModel : RegisterModel
{
    public override string Name=>"cas-register";
    public override void Validate(Operation o)
    {
        if(o.Kind=="cas")
        {
            if(o.Args.Length!=2 || o.Result is not ("true" or "false"))throw new ArgumentException("CAS requires expected,new and a boolean result.");
        }
        else base.Validate(o);
    }
    public override bool Apply(string state,Operation o,out string next,out string reason)
    {
        if(o.Kind!="cas")return base.Apply(state,o,out next,out reason);
        var matches=state==o.Args[0];next=matches?o.Args[1]:state;
        reason="compare/exchange success flag contradicts the observed prior value";
        return o.Result==(matches?"true":"false");
    }
}
public static class Models
{
    public static IModel Get(string name)=>name switch
    {
        "register"=>new RegisterModel(),"cas-register"=>new CasRegisterModel(),"counter"=>new CounterModel(),
        "set"=>new SetModel(),"queue"=>new QueueModel(),"stack"=>new StackModel(),"map"=>new MapModel(),
        _=>throw new ArgumentException("Unknown model; expected register, cas-register, counter, set, queue, stack or map.")
    };
}
