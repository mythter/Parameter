using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

using Parameter.Services.Interfaces;

namespace Parameter.Services
{
	public class PlatformServicesAccessor(IClassicDesktopStyleApplicationLifetime applicationLifetime) : IPlatformServicesAccessor
	{
		public IClipboard? Clipboard => applicationLifetime.MainWindow?.Clipboard;
	}
}
