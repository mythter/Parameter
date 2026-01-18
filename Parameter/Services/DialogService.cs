using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

using Myth.Avalonia.Controls;
using Myth.Avalonia.Controls.Enums;
using Myth.Avalonia.Controls.Options;

using Parameter.Services.Interfaces;

namespace Parameter.Services
{
	public class DialogService(Window mainWindow) : IDialogService
	{
		public async Task<IStorageFile?> ShowAwsCredentialsFileDialogAsync()
		{
			var options = new FilePickerOpenOptions
			{
				Title = "Choose AWS credentials file",
				AllowMultiple = false
			};

			var result = await mainWindow.StorageProvider.OpenFilePickerAsync(options);

			return result.FirstOrDefault();
		}

		public Task ShowInfoAsync(string message, string? title = null, bool isTextSelectable = false)
		{
			return MessageBox.ShowDialog(mainWindow,
				new MessageBoxOptions()
				{
					Title = title ?? "Information",
					Message = message,
					Icon = MessageBoxIcon.Info,
					IsTextSelectable = isTextSelectable
				});
		}

		public Task ShowWarningAsync(string message, string? title = null, bool isTextSelectable = false)
		{
			return MessageBox.ShowDialog(mainWindow,
				new MessageBoxOptions()
				{
					Title = title ?? "Warning",
					Message = message,
					Icon = MessageBoxIcon.Warning,
					IsTextSelectable = isTextSelectable
				});
		}

		public Task ShowErrorAsync(string message, string? title = null, bool isTextSelectable = true)
		{
			return MessageBox.ShowDialog(mainWindow,
				new MessageBoxOptions()
				{
					Title = title ?? "Error occurred",
					Message = message,
					Icon = MessageBoxIcon.Error,
					IsTextSelectable = isTextSelectable
				});
		}
	}
}
