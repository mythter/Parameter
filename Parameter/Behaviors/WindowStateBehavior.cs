using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

using Parameter.Services;
using Parameter.Services.Interfaces;

namespace Parameter.Behaviors
{
	public class WindowStateBehavior : Behavior<Window>
	{
		protected override void OnAttached()
		{
			base.OnAttached();

			if (AssociatedObject is not null)
			{
				AssociatedObject.Opened += OnWindowOpened;
				AssociatedObject.Closing += OnWindowClosing;
			}
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (AssociatedObject is not null)
			{
				AssociatedObject.Opened -= OnWindowOpened;
				AssociatedObject.Closing -= OnWindowClosing;
			}
		}

		private void OnWindowOpened(object? sender, EventArgs e)
		{
			if (AssociatedObject is not Window window) return;

			var settingsService = ServiceManager.TryGetService<ISettingsService>();

			if (settingsService == null) return;

			var settings = settingsService.Settings.Window;

			// restore size and position
			if (settings.Width > 0 && settings.Height > 0)
			{
				window.Width = settings.Width;
				window.Height = settings.Height;
			}

			const double Tolerance = 0.0001;
			if (Math.Abs(settings.X) > Tolerance || Math.Abs(settings.Y) > Tolerance)
			{
				window.Position = new PixelPoint((int)settings.X, (int)settings.Y);
			}

			// restore state
			if (settings.IsMaximized)
			{
				window.WindowState = WindowState.Maximized;
			}
		}

		private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
		{
			if (AssociatedObject is not Window window) return;

			var settingsService = ServiceManager.TryGetService<ISettingsService>();

			if (settingsService == null) return;

			SaveWindowState(window);

			settingsService.Save();
		}

		private static void SaveWindowState(Window window)
		{
			var settingsService = ServiceManager.TryGetService<ISettingsService>();

			if (settingsService == null) return;

			var settings = settingsService.Settings.Window;

			// save size and position only if not maximized
			if (window.WindowState != WindowState.Maximized)
			{
				settings.Width = window.Bounds.Width;
				settings.Height = window.Bounds.Height;
				settings.X = window.Position.X;
				settings.Y = window.Position.Y;
			}

			settings.IsMaximized = window.WindowState == WindowState.Maximized;
		}
	}
}
