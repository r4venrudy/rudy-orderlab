using Rudy.OrderLab;
internal static partial class Program
{
    private static void FileTests()
    {
        var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".json");
        try
        {
            File.WriteAllText(path,"{}");using(var file=new HistoryFile(path)){Assert(file.Bytes.Length==2 && file.Sha256.Length==64,"bounded source identity");file.EnsureStable();}
            using(var file=File.Create(path))file.SetLength(HistoryReader.MaxBytes+1L);
            Throws(()=>{using var _=new HistoryFile(path);},"oversize source before allocation");
        }
        finally {File.Delete(path);}
    }
}
