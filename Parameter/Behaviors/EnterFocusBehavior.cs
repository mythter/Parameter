using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Parameter.Behaviors
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1121:Assignments should not be made from within sub-expressions", Justification = "<Pending>")]
	public class EnterFocusBehavior : Behavior<Control>
	{
		public static readonly StyledProperty<IInputElement?> NextFocusProperty =
			AvaloniaProperty.Register<EnterFocusBehavior, IInputElement?>(nameof(NextFocus));

		public IInputElement? NextFocus
		{
			get => GetValue(NextFocusProperty);
			set => SetValue(NextFocusProperty, value);
		}

		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject?.KeyDown += OnKeyDown;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			AssociatedObject?.KeyDown -= OnKeyDown;
		}

		private void OnKeyDown(object? sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && NextFocus != null)
			{
				NextFocus.Focus();
				e.Handled = true;
			}
		}
	}
}
