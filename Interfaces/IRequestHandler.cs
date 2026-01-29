namespace EndfieldBot.Interfaces;

public interface IRequestHandler
{
    public Task<T?> GetAsync<T>(string url);
}