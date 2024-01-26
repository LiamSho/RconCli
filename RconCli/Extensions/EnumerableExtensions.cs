namespace RconCli.Extensions;

public static class EnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> enumerable)
    {
        return (await enumerable).ToList();
    }
}
