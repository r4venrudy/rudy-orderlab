using Rudy.OrderLab;
internal static partial class Program
{
    private static void PartitionTests()
    {
        var h=new History(1,"register",["0"],[Op("a",0,5,"write",null,"1") with{Object="A"},Op("b",1,2,"read","0") with{Object="B"},Op("c",3,4,"read","0") with{Object="A"}]);
        var r=PartitionChecker.Check(h,new RegisterModel());
        Assert(r.Verdict==Verdict.Linearizable && WitnessVerifier.Verify(h,new RegisterModel(),r.Witness),"partition witness merged");
        Assert(PartitionChecker.Check(h,new RegisterModel(),new(1)).Verdict==Verdict.Inconclusive,"global partition budget");
        var malformed=new History(1,"register",["0"],[Op("bad-read",0,1,"read","never-written") with{Object="A"},Op("bad-schema",2,3,"execute",null) with{Object="B"}]);
        Throws(()=>PartitionChecker.Check(malformed,new RegisterModel()),"an early rejecting object cannot hide malformed later operations");
        Throws(()=>PartitionChecker.Check(h,new CounterModel()),"partition model mismatch");
    }
}
