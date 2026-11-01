using System.Globalization;
namespace Rudy.OrderLab;
public static class ConcurrentDemo
{
    public static History Record(int workers=4,int operationsPerWorker=8)
    {
        if(workers<1 || workers>8 || operationsPerWorker<1 || workers*operationsPerWorker>128)throw new ArgumentException("Demo requires 1..8 workers and 1..128 total increments.");
        var recorder=new HistoryRecorder("counter","0");long value=0;
        Task.WaitAll(Enumerable.Range(0,workers).Select(worker=>Task.Run(()=>
        {
            for(int i=0;i<operationsPerWorker;i++)
            {
                var id=recorder.Begin("worker-"+worker.ToString(CultureInfo.InvariantCulture),"atomic-counter","add","1");
                var observed=Interlocked.Increment(ref value);
                Thread.Yield();
                recorder.Complete(id,observed.ToString(CultureInfo.InvariantCulture));
            }
        })).ToArray());
        return recorder.Snapshot();
    }
}
