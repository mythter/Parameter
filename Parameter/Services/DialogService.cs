using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

using Myth.Avalonia.Controls;
using Myth.Avalonia.Controls.Enums;
using Myth.Avalonia.Controls.Options;

using Parameter.Services.Interfaces;
using Parameter.Views;

namespace Parameter.Services
{
	public class DialogService(IClassicDesktopStyleApplicationLifetime applicationLifetime) : IDialogService
	{
		private MainWindow MainWindow => (MainWindow)applicationLifetime.MainWindow!;

		public async Task<IStorageFile?> ShowAwsCredentialsFileDialogAsync()
		{
			var options = new FilePickerOpenOptions
			{
				Title = "Choose AWS credentials file",
				AllowMultiple = false
			};

			var result = await MainWindow.StorageProvider.OpenFilePickerAsync(options);

			return result.FirstOrDefault();
		}

		public Task ShowInfoAsync(string message, string? title = null, bool isTextSelectable = false)
		{
			return MessageBox.ShowDialog(MainWindow,
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
			return MessageBox.ShowDialog(MainWindow,
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
			return MessageBox.ShowDialog(MainWindow,
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
