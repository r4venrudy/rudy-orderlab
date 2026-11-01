using System.Text;
using Rudy.OrderLab;
internal static partial class Program
{
    private static void MutationTests()
    {
        var h=new History(1,"register",["0"],[Op("w",0,3,"write",null,"1"),Op("r",1,2,"read","0")]);
        var json=HistoryWriter.Json(h);var bytes=Encoding.UTF8.GetBytes(json);
        var closing=json.LastIndexOf('}');
        for(int i=0;i<=closing;i++)
        {var length=i;Throws(()=>HistoryReader.Read(bytes.AsMemory(0,length)),"every-byte truncation before final object close");}
        var random=new Random(67181);
        for(int trial=0;trial<1200;trial++)
        {
            var changed=(byte[])bytes.Clone();for(int j=0;j<1+trial%4;j++)changed[random.Next(changed.Length)]=(byte)random.Next(256);
            try {var parsed=HistoryReader.Read(changed);var r=Checker.Check(parsed,Models.Get(parsed.Model),new(2000,500));Assert(Enum.IsDefined(r.Verdict),"mutation gives typed verdict");}
            catch(ArgumentException){Assert(true,"mutation rejected by contract");}
        }
        Throws(()=>HistoryReader.Read(Encoding.UTF8.GetBytes(new string('[',20)+new string(']',20))),"bounded nesting");
        Throws(()=>HistoryReader.Validate(h with{Initial=["bad\ud800"]}),"unpaired Unicode scalar");
        Throws(()=>HistoryReader.Validate(h with{Operations=Enumerable.Range(0,257).Select(i=>Op(i.ToString(),i*2,i*2+1,"read","0")).ToArray()}),"bounded operation count");
    }
}
