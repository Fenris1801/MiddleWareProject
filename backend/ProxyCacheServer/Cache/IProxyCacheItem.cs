namespace ProxyCacheServer
{
    public interface IProxyCacheItem
    {
        Task FillFromWebAsync(params string[] args);
    }
}
