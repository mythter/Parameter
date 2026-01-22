using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;

using Parameter.Services;
using Parameter.Services.Implementations;
using Parameter.Services.Interfaces;
using Parameter.ViewModels;
using Parameter.Views;

namespace Parameter;

public partial class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		// Line below is needed to remove Avalonia data validation.
		// Without this line you will get duplicate validations from both Avalonia and CT
		BindingPlugins.DataValidators.RemoveAt(0);

		var collection = new ServiceCollection();

		collection.AddSingleton<MainViewModel>();

		collection.AddSingleton<IAwsProfilesService, AwsProfilesService>();
		collection.AddSingleton<IParameterServiceFactory, ParameterServiceFactory>();

		collection.AddSingleton<ISettingsService, SettingsService>();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			collection.AddSingleton<IDialogService>(new DialogService(desktop));
			collection.AddSingleton<IPlatformServicesAccessor>(new PlatformServicesAccessor(desktop));

			var services = collection.BuildServiceProvider();

			ServiceManager.Initialize(services);

			var settingsService = ServiceManager.GetService<ISettingsService>();

			settingsService.Load();

			desktop.MainWindow = new MainWindow();

			desktop.MainWindow.DataContext = ServiceManager.GetService<MainViewModel>();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
