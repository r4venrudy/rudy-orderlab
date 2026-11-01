namespace Rudy.OrderLab;

public class RegisterModel : IModel
{
    public virtual string Name=>"register";
    public string Initial(string[] values)
    { if(values.Length!=1) throw new ArgumentException("Register needs one initial value."); return values[0]; }
    public virtual void Validate(Operation o)
    {
        if(!((o.Kind=="write" && o.Args.Length==1 && o.Result is null) ||
            (o.Kind=="read" && o.Args.Length==0 && o.Result is not null)))
            throw new ArgumentException("Register requires write(value)->null or read()->string.");
    }
    public virtual bool Apply(string state, Operation o, out string next, out string reason)
    {
        next=state;reason="observed result disagrees with sequential register";
        if(o.Kind=="write") { next=o.Args[0];return true; }
        return o.Result==state;
    }
}
