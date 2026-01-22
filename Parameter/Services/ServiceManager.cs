using System;

using Microsoft.Extensions.DependencyInjection;

namespace Parameter.Services
{
	public static class ServiceManager
	{
		private static IServiceProvider? _serviceProvider;

		public static void Initialize(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public static T GetService<T>() where T : notnull
		{
			if (_serviceProvider == null)
				throw new InvalidOperationException("ServiceManager is not initialized.");

			return _serviceProvider.GetRequiredService<T>();
		}

		public static T? TryGetService<T>() where T : class
		{
			return _serviceProvider?.GetService<T>();
		}
	}
}
