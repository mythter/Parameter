using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Parameter.Behaviors
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1121:Assignments should not be made from within sub-expressions", Justification = "<Pending>")]
	public class AutoCompleteKeyDownBehavior : Behavior<AutoCompleteBox>
	{
		public static readonly StyledProperty<ICommand?> CommandProperty =
			AvaloniaProperty.Register<AutoCompleteKeyDownBehavior, ICommand?>(nameof(Command));

		public ICommand? Command
		{
			get => GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		protected override void OnAttached()
		{
			AssociatedObject?.KeyDown += OnKeyDown;
		}

		protected override void OnDetaching()
		{
			AssociatedObject?.KeyDown -= OnKeyDown;
		}

		private void OnKeyDown(object? sender, KeyEventArgs e)
		{
			if (Command?.CanExecute(e) == true)
				Command.Execute(e);
		}
	}
}
