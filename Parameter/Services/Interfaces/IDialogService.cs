using System.Threading.Tasks;

using Avalonia.Platform.Storage;

namespace Parameter.Services.Interfaces
{
	public interface IDialogService
	{
		public Task<IStorageFile?> ShowAwsCredentialsFileDialogAsync();

		Task ShowInfoAsync(string message, string? title = null, bool isTextSelectable = false);

		Task ShowWarningAsync(string message, string? title = null, bool isTextSelectable = false);

		Task ShowErrorAsync(string message, string? title = null, bool isTextSelectable = true);
	}
}
