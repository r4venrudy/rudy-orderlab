using Rudy.OrderLab;
internal static partial class Program
{
    private static void QueueTests()
    {
        var m=new QueueModel();var s=m.Initial(["first","second"]);
        Assert(m.Apply(s,Op("d",0,1,"dequeue","first"),out var n,out _) && m.Apply(n,Op("p",2,3,"peek","second"),out _,out _),"FIFO");
        Assert(!m.Apply(s,Op("d",0,1,"dequeue","second"),out _,out _),"FIFO inversion");
        Assert(m.Apply(m.Initial([]),Op("d",0,1,"dequeue",null),out _,out _),"empty queue");
    }
}
