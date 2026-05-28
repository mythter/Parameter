using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;

using Parameter.Models;
using Parameter.Services;
using Parameter.Services.Implementations;
using Parameter.Services.Interfaces;
using Parameter.ViewModels;
using Parameter.Views;

namespace Parameter;

public partial class App : Application
{
	private ServiceProvider? _serviceProvider;

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		var collection = new ServiceCollection();

		collection.AddSingleton<MainViewModel>();

		collection.AddSingleton<IAwsProfilesService, AwsProfilesService>();
		collection.AddSingleton<IParameterServiceFactory, ParameterServiceFactory>();

		collection.AddSingleton<IAppDataProvider<AppData>, AppDataProvider>();
		collection.AddSingleton<IWindowStateService, WindowStateService>();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			collection.AddSingleton<IPlatformServicesAccessor>(new PlatformServicesAccessor(desktop));

			_serviceProvider = collection.BuildServiceProvider();

			ServiceManager.Initialize(_serviceProvider);

			var appDataProvider = ServiceManager.GetService<IAppDataProvider<AppData>>();

			appDataProvider.Load();

			desktop.MainWindow = new MainWindow(
				appDataProvider,
				_serviceProvider.GetRequiredService<IWindowStateService>())
			{
				DataContext = ServiceManager.GetService<MainViewModel>()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}

	private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
	{
		// Disposing the ServiceProvider disposes singletons that implement IDisposable
		// (PortForwardManager — closes all SSH connections; PrivateKeyCache — wipes keys).
		_serviceProvider?.Dispose();
		_serviceProvider = null;
	}
}
