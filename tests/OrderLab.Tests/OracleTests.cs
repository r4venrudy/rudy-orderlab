using System.Globalization;
using Rudy.OrderLab;
internal static partial class Program
{
    // Enumerates permutations; independent of masks, model classes, memoization,
    // partitioning and ObjectState. Small histories only, as a differential oracle.
    private static bool Brute(History h)
    {
        bool Walk(List<Operation> left,Dictionary<string,List<string>> states)
        {
            if(left.Count==0)return true;
            foreach(var o in left)
            {
                if(left.Any(p=>p.End<o.Start))continue;
                var s=states.TryGetValue(o.Object,out var prior)?new List<string>(prior):new List<string>(h.Initial);
                string? expected=null;
                switch(h.Model)
                {
                    case "register":case "cas-register":
                        if(o.Kind=="write")s[0]=o.Args[0];
                        else if(o.Kind=="cas"){var same=s[0]==o.Args[0];expected=same?"true":"false";if(same)s[0]=o.Args[1];}
                        else expected=s[0];break;
                    case "counter":
                        var n=long.Parse(s[0],CultureInfo.InvariantCulture);if(o.Kind=="add")n+=long.Parse(o.Args[0],CultureInfo.InvariantCulture);
                        expected=n.ToString(CultureInfo.InvariantCulture);s[0]=expected;break;
                    case "queue":case "stack":
                        if(o.Kind is "enqueue" or "push")s.Add(o.Args[0]);
                        else if(s.Count>0){var at=h.Model=="queue"?0:s.Count-1;expected=s[at];if(o.Kind!="peek")s.RemoveAt(at);}break;
                    case "set":
                        if(o.Kind=="size")expected=s.Count.ToString(CultureInfo.InvariantCulture);
                        else {var present=s.Contains(o.Args[0]);expected=(o.Kind=="add"?!present:present)?"true":"false";if(o.Kind=="add"&&!present)s.Add(o.Args[0]);if(o.Kind=="remove")s.Remove(o.Args[0]);}break;
                    case "map":
                        var index=s.IndexOf(o.Args[0]); // generated keys never occur as values
                        if(o.Kind=="put"){if(index<0){s.Add(o.Args[0]);s.Add(o.Args[1]);}else s[index+1]=o.Args[1];}
                        else if(o.Kind=="get")expected=index<0?null:s[index+1];
                        else {expected=index<0?"false":"true";if(index>=0)s.RemoveRange(index,2);}break;
                }
                if(expected!=o.Result)continue;
                var next=new Dictionary<string,List<string>>(states,StringComparer.Ordinal){[o.Object]=s};
                if(Walk(left.Where(p=>p.Id!=o.Id).ToList(),next))return true;
            }
            return false;
        }
        return Walk(h.Operations.ToList(),new(StringComparer.Ordinal));
    }
    private static void OracleTests()
    {
        var random=new Random(197901);
        foreach(var model in new[]{"register","cas-register","counter","queue","stack","set","map"})
        for(int trial=0;trial<160;trial++)
        {
            var count=random.Next(1,7);var ticks=Enumerable.Range(0,count*2).OrderBy(_=>random.Next()).ToArray();var ops=new List<Operation>();
            string Value()=>random.Next(3).ToString(CultureInfo.InvariantCulture);
            string Bool()=>random.Next(2)==0?"true":"false";
            for(int i=0;i<count;i++)
            {
                var which=random.Next(3);string kind;string? result;string[] args;
                if(model is "register" or "cas-register")
                {kind=which==0?"write":which==1&&model=="cas-register"?"cas":"read";args=kind=="write"?[Value()]:kind=="cas"?[Value(),Value()]:[];result=kind=="write"?null:kind=="cas"?Bool():Value();}
                else if(model=="counter"){kind=which==0?"read":"add";args=kind=="read"?[]:["1"];result=Value();}
                else if(model is "queue" or "stack"){kind=which==0?(model=="queue"?"enqueue":"push"):which==1?(model=="queue"?"dequeue":"pop"):"peek";args=which==0?[Value()]:[];result=which==0||random.Next(3)==0?null:Value();}
                else if(model=="set"){kind=new[]{"add","remove","contains"}[which];args=[Value()];result=Bool();}
                else {kind=new[]{"put","get","delete"}[which];args=which==0?["key-"+Value(),Value()]:["key-"+Value()];result=which==0?null:which==1?(random.Next(3)==0?null:Value()):Bool();}
                ops.Add(Op(i.ToString(CultureInfo.InvariantCulture),Math.Min(ticks[i*2],ticks[i*2+1]),Math.Max(ticks[i*2],ticks[i*2+1]),kind,result,args) with{Object=random.Next(2)==0?"A":"B"});
            }
            var h=new History(1,model,model is "register" or "cas-register" or "counter"?["0"]:[],ops.ToArray());var expected=Brute(h);var actual=Checker.Check(h,Models.Get(model));var partition=PartitionChecker.Check(h,Models.Get(model));
            Assert(actual.Verdict==(expected?Verdict.Linearizable:Verdict.NotLinearizable),"permutation oracle / "+model);
            Assert(partition.Verdict==actual.Verdict,"partition vs exhaustive / "+model);
            if(expected)Assert(WitnessVerifier.Verify(h,Models.Get(model),actual.Witness),"oracle-positive witness");
        }
    }
}
