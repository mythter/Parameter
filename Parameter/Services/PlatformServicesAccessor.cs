using Avalonia.Controls;
using Avalonia.Input.Platform;

using Parameter.Services.Interfaces;

namespace Parameter.Services
{
	public class PlatformServicesAccessor(Window mainWindow) : IPlatformServicesAccessor
	{
		public IClipboard? Clipboard => mainWindow.Clipboard;
	}
}
