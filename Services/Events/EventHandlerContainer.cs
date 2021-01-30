using Contracts;
using Models.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Events
{
	public class EventHandlerContainer
	{
		private readonly IServiceProvider _serviceProvider;
		private static readonly Dictionary<Type, List<Type>> _mappings = new Dictionary<Type, List<Type>>();

		public EventHandlerContainer(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public static void Subscribe<T, THandler>()
			where T : EventBase
			where THandler : IEventHandler<T>
		{
			var name = typeof(T);

			if (!_mappings.ContainsKey(name))
			{
				_mappings.Add(name, new List<Type> { });
			}

			_mappings[name].Add(typeof(THandler));
		}

		public static void Unsubscribe<T, THandler>()
			where T : EventBase
			where THandler : IEventHandler<T>
		{
			var name = typeof(T);
			_mappings[name].Remove(typeof(THandler));

			if (_mappings[name].Count == 0)
			{
				_mappings.Remove(name);
			}
		}

		public async Task PublishAsync<T>(T o) where T : EventBase
		{
			var name = typeof(T);

			if (_mappings.ContainsKey(name))
			{
				foreach (var handler in _mappings[name])
				{
					var service = (IEventHandler<T>)_serviceProvider.GetService(handler);
					//TODO: Execute events in background
					await service.RunAsync(o);
				}
			}
		}
	}
}