using System.Collections.Concurrent;
using Rudy.OrderLab;

if(args.Length!=1)throw new ArgumentException("Pass a new output history path.");
var queue=new ConcurrentQueue<string>();var recorder=new HistoryRecorder("queue");
Task.WaitAll(Enumerable.Range(0,4).Select(worker=>Task.Run(()=>
{
    for(int i=0;i<2;i++)
    {
        var client="worker-"+worker;var value=client+"/"+i;
        var enqueue=recorder.Begin(client,"queue","enqueue",value);
        queue.Enqueue(value);recorder.Complete(enqueue,null);
        var dequeue=recorder.Begin(client,"queue","dequeue");
        var ok=queue.TryDequeue(out var observed);recorder.Complete(dequeue,ok?observed:null);
    }
})).ToArray());
var history=recorder.Snapshot();var result=Checker.Check(history,new QueueModel());
ReportOutput.Write(HistoryWriter.Json(history),args[0]);
Console.WriteLine($"ConcurrentQueue history: {result.Verdict}; {history.Operations.Length} operations; witness replay: {WitnessVerifier.Verify(history,new QueueModel(),result.Witness)}");
return result.Verdict==Verdict.Linearizable?0:2;
