using Avalonia.Input.Platform;

namespace Parameter.Services.Interfaces
{
	public interface IPlatformServicesAccessor
	{
		IClipboard? Clipboard { get; }
	}
}
