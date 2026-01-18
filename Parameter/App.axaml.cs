using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

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

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow();

			collection.AddSingleton<IDialogService>(new DialogService(desktop.MainWindow));
			collection.AddSingleton<IPlatformServicesAccessor>(new PlatformServicesAccessor(desktop.MainWindow));

			var services = collection.BuildServiceProvider();

			desktop.MainWindow.DataContext = services.GetRequiredService<MainViewModel>();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
