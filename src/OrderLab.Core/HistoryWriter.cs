using System.Text.Json;
namespace Rudy.OrderLab;
public static class HistoryWriter
{
    public static string Json(History history)
    {
        HistoryReader.Validate(history);
        return JsonSerializer.Serialize(history,new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase,WriteIndented=true})+"\n";
    }
}
