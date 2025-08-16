namespace MedTheMediator.Utils;

internal static class TaskCaster
{
    public static async Task<object> Cast<T>(Task<T> task) => await task.ConfigureAwait(false)!;
}