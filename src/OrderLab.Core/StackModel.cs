namespace Rudy.OrderLab;
/// <summary>Initial values are bottom to top; pushes append to the top.</summary>
public sealed class StackModel : QueueModel
{
    public override string Name=>"stack";
    protected override string Insert=>"push";
    protected override string Remove=>"pop";
    protected override int Head(int count)=>count-1;
}
