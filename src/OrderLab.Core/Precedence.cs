using System.Numerics;
namespace Rudy.OrderLab;

/// <summary>Only nonoverlap imposes real-time order; overlapping operations can commute.</summary>
public static class Precedence
{
    public static BigInteger[] Build(History history)
    {
        HistoryReader.Validate(history);
        var ops=history.Operations;
        var edges=new BigInteger[ops.Length];
        for(int i=0;i<ops.Length;i++) for(int j=0;j<ops.Length;j++)
            if(ops[j].End<ops[i].Start) edges[i]|=BigInteger.One<<j;
        return edges;
    }
    public static bool Enabled(BigInteger predecessors, BigInteger done) => (predecessors & done)==predecessors;
}
