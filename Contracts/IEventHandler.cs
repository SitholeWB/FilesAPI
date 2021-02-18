using Models.Events;
using System.Threading.Tasks;

namespace Contracts
{
	public interface IEventHandler<in T> where T : EventBase
	{
		Task RunAsync(T obj);
	}
}