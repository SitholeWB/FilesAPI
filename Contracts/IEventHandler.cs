namespace Contracts;

public interface IEventHandler<in T> where T : EventBase
{
    Task RunAsync(T obj, CancellationToken token);
}