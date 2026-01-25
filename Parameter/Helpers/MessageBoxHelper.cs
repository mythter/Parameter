using System.Threading.Tasks;
using Myth.Avalonia.Controls;
using Myth.Avalonia.Controls.Enums;
using Myth.Avalonia.Controls.Options;
using Myth.Avalonia.Services.Abstractions;

namespace Parameter.Helpers
{
	internal static class MessageBoxHelper
	{
		public static Task<MessageBoxResult> ShowWarningMessageBoxDialog(this IDialogContext context, string message, string? title = null, bool isTextSelectable = false)
		{
			return MessageBox.ShowMessageBoxDialog(context,
				new MessageBoxOptions()
				{
					Title = title ?? "Warning",
					Message = message,
					Icon = MessageBoxIcon.Warning,
					IsTextSelectable = isTextSelectable
				});
		}

		public static Task<MessageBoxResult> ShowErrorMessageBoxDialog(this IDialogContext context, string message, string? title = null, bool isTextSelectable = true)
		{
			return MessageBox.ShowMessageBoxDialog(context,
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
