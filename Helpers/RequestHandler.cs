using System.Text.Json;
using EndfieldBot.Interfaces;
using Microsoft.Extensions.Logging;

namespace EndfieldBot.Helpers;

public class RequestHandler(
    HttpClient httpClient
) : IRequestHandler
{
    public async Task<T?> GetAsync<T>(string url)
    {
        var request = await httpClient.GetAsync(url);
        var response = await request.Content.ReadAsStringAsync();
        var deserialized = JsonSerializer.Deserialize<T>(response);
        return deserialized;
    }
}