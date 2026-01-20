using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Parameter.AttachedProperties
{
	public static class EnterFocusBehavior
	{
		public static readonly AttachedProperty<IInputElement?> NextFocusProperty =
			AvaloniaProperty.RegisterAttached<Control, Control, IInputElement?>("NextFocus");

		static EnterFocusBehavior()
		{
			InputElement.KeyDownEvent.AddClassHandler<Control>(OnKeyDown);
		}

		private static void OnKeyDown(Control control, KeyEventArgs e)
		{
			if (e.Key == Key.Enter &&
				GetNextFocus(control) is IInputElement next)
			{
				next.Focus();
				e.Handled = true;
			}
		}

		public static void SetNextFocus(Control control, IInputElement value) =>
			control.SetValue(NextFocusProperty, value);

		public static IInputElement? GetNextFocus(Control control) =>
			control.GetValue(NextFocusProperty);
	}
}
