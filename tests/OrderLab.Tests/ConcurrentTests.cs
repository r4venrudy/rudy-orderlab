using Rudy.OrderLab;
internal static partial class Program
{
    private static void ConcurrentTests()
    {
        for(int i=0;i<8;i++)
        {
            var h=ConcurrentDemo.Record(4,6);var r=Checker.Check(h,new CounterModel());
            Assert(h.Operations.Length==24 && r.Verdict==Verdict.Linearizable && WitnessVerifier.Verify(h,new CounterModel(),r.Witness),"real Interlocked recording and replay");
        }
    }
}
