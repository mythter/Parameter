using System.Diagnostics.CodeAnalysis;

using Avalonia;

namespace Parameter;

[SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors", Justification = "Entry point class")]
class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args) => BuildAvaloniaApp()
		.StartWithClassicDesktopLifetime(args);

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithDeveloperTools()
			.WithInterFont()
			.LogToTrace();

}
