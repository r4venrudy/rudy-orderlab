using System.Security.Cryptography;
namespace Rudy.OrderLab;
public sealed class HistoryFile : IDisposable
{
    private readonly FileStream stream;
    public byte[] Bytes { get; }
    public string Sha256 { get; }
    public HistoryFile(string path)
    {
        stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
        try
        {
            if(!stream.CanSeek || stream.Length>HistoryReader.MaxBytes)throw new ArgumentException("Expected a seekable history of at most 8 MiB.");
            Bytes=new byte[checked((int)stream.Length)];stream.ReadExactly(Bytes);
            if(stream.ReadByte()!=-1)throw new ArgumentException("History changed while reading.");
            Sha256=Convert.ToHexStringLower(SHA256.HashData(Bytes));
        }
        catch {stream.Dispose();throw;}
    }
    public void EnsureStable()
    {
        if(stream.Length!=Bytes.Length)throw new ArgumentException("History length changed during analysis.");
        stream.Position=0;var copy=new byte[Bytes.Length];stream.ReadExactly(copy);
        if(stream.ReadByte()!=-1 || !CryptographicOperations.FixedTimeEquals(SHA256.HashData(copy),SHA256.HashData(Bytes)))throw new ArgumentException("History content changed during analysis.");
    }
    public void Dispose()=>stream.Dispose();
}
