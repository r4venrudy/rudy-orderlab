using Rudy.OrderLab;
internal static partial class Program
{
    private static void ObjectTests()
    {
        var h=new History(1,"register",["0"],[Op("a",0,1,"write",null,"one") with{Object="A"},Op("b",2,3,"read","0") with{Object="B"},Op("c",4,5,"read","one") with{Object="A"}]);
        var r=Checker.Check(h,new RegisterModel());
        Assert(r.Verdict==Verdict.Linearizable && WitnessVerifier.Verify(h,new RegisterModel(),r.Witness),"isolated object states preserve cross-object real time");
        Assert(Checker.Check(h with{Operations=[..h.Operations,Op("d",6,7,"read","one") with{Object="B"}]},new RegisterModel()).Verdict==Verdict.NotLinearizable,"no state leak between objects");
    }
}
