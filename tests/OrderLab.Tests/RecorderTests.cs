using Rudy.OrderLab;
internal static partial class Program
{
    private static void RecorderTests()
    {
        var recorder=new HistoryRecorder("register","0");var id=recorder.Begin("worker","main","write","1");
        Throws(()=>recorder.Snapshot(),"pending operation cannot disappear");
        Throws(()=>recorder.Begin("worker","main","read"),"one pending per sequential worker");
        recorder.Complete(id,null);Throws(()=>recorder.Complete(id,null),"no double completion");
        var snapshot=recorder.Snapshot();snapshot.Initial[0]="mutated";
        Assert(recorder.Snapshot().Initial[0]=="0","snapshot ownership");
        Assert(Checker.Check(recorder.Snapshot(),new RegisterModel()).Verdict==Verdict.Linearizable,"recorded invocation/response");
    }
}
