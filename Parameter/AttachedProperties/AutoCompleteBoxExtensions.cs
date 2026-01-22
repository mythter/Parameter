using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;

namespace Parameter.AttachedProperties
{
	public static class AutoCompleteBoxExtensions
	{
		public static readonly AttachedProperty<ICommand?> RemoveItemCommandProperty =
			AvaloniaProperty.RegisterAttached<AutoCompleteBox, Control, ICommand?>(
				"RemoveItemCommand");

		public static void SetRemoveItemCommand(Control element, ICommand? value) =>
			element.SetValue(RemoveItemCommandProperty, value);

		public static ICommand? GetRemoveItemCommand(Control element) =>
			element.GetValue(RemoveItemCommandProperty);
	}
}
