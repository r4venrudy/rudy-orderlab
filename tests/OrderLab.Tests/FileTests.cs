using Rudy.OrderLab;
internal static partial class Program
{
    private static void FileTests()
    {
        var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".json");
        try
        {
            File.WriteAllText(path,"{}");using(var file=new HistoryFile(path)){Assert(file.Bytes.Length==2 && file.Sha256.Length==64,"bounded source identity");file.EnsureStable();}
            bool exists=false;try{ReportOutput.Write("overwrite",path);}catch(IOException){exists=true;}
            Assert(exists && File.ReadAllText(path)=="{}","never overwrite an existing file");
            var report=path+".report";ReportOutput.Write("new",report);Assert(File.ReadAllText(report)=="new","new output");
            if(!OperatingSystem.IsWindows())Assert((File.GetUnixFileMode(report)&(UnixFileMode.GroupRead|UnixFileMode.OtherRead))==0,"private POSIX report");
            File.Delete(report);
            using(var file=File.Create(path))file.SetLength(HistoryReader.MaxBytes+1L);
            Throws(()=>{using var _=new HistoryFile(path);},"oversize source before allocation");
        }
        finally {File.Delete(path);}
    }
}
