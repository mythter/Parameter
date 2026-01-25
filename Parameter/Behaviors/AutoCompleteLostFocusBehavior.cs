using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace Parameter.Behaviors
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1121:Assignments should not be made from within sub-expressions", Justification = "<Pending>")]
	public sealed class AutoCompleteLostFocusBehavior : Behavior<AutoCompleteBox>
	{
		public static readonly StyledProperty<ICommand?> CommandProperty =
			AvaloniaProperty.Register<AutoCompleteLostFocusBehavior, ICommand?>(nameof(Command));

		public ICommand? Command
		{
			get => GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public static readonly StyledProperty<object?> CommandParameterProperty =
			AvaloniaProperty.Register<AutoCompleteLostFocusBehavior, object?>(
				nameof(CommandParameter));

		public object? CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		protected override void OnAttached()
		{
			AssociatedObject?.LostFocus += OnLostFocus;
		}

		protected override void OnDetaching()
		{
			AssociatedObject?.LostFocus -= OnLostFocus;
		}

		private void OnLostFocus(object? sender, RoutedEventArgs e)
		{
			if (TopLevel.GetTopLevel(AssociatedObject) is { } topLevel &&
				topLevel?.FocusManager?.GetFocusedElement() is Visual visual &&
				visual?.FindAncestorOfType<PopupRoot>() is not null)
			{
				return;
			}

			var parameter = CommandParameter ?? AssociatedObject?.Text;

			if (Command?.CanExecute(parameter) == true)
			{
				Command.Execute(parameter);
			}
		}
	}
}
