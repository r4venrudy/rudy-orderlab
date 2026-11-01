using System.Text;
namespace Rudy.OrderLab;
public static class ReportOutput
{
    public static void Write(string content,string? path=null)
    {
        var bytes=new UTF8Encoding(false,true).GetBytes(content);
        if(bytes.Length>16*1024*1024)throw new ArgumentException("Report exceeds 16 MiB.");
        if(path is null){Console.OutputEncoding=new UTF8Encoding(false);Console.Write(content);return;}
        var options=new FileStreamOptions{Mode=FileMode.CreateNew,Access=FileAccess.Write,Share=FileShare.None};
        if(!OperatingSystem.IsWindows())options.UnixCreateMode=UnixFileMode.UserRead|UnixFileMode.UserWrite;
        using var file=new FileStream(path,options);file.Write(bytes);
    }
}
