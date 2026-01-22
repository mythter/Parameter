using System;

using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

using Parameter.Models;
using Parameter.Services;
using Parameter.Services.Interfaces;

namespace Parameter.Behaviors
{
	public class DataGridStateBehavior : Behavior<DataGrid>
	{
		protected override void OnAttached()
		{
			base.OnAttached();

			if (AssociatedObject != null)
			{
				AssociatedObject.Loaded += OnDataGridLoaded;
			}
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (AssociatedObject is not null)
			{
				AssociatedObject.Loaded -= OnDataGridLoaded;

				SaveColumnSettings(AssociatedObject);
			}
		}

		private void OnDataGridLoaded(object? sender, EventArgs e)
		{
			if (AssociatedObject is not null)
			{
				RestoreColumnSettings(AssociatedObject);
			}
		}

		private static void RestoreColumnSettings(DataGrid? dataGrid)
		{
			var settingsService = ServiceManager.TryGetService<ISettingsService>();

			if (settingsService is null || dataGrid?.Name is null) return;

			var settings = settingsService.Settings;

			if (!settings.DataGrids.TryGetValue(dataGrid.Name, out var gridSettings))
				return;

			// restore column order
			foreach (var column in dataGrid.Columns)
			{
				var header = column.Header?.ToString();
				if (header == null) continue;

				if (gridSettings.ColumnOrder.TryGetValue(header, out var displayIndex))
				{
					column.DisplayIndex = displayIndex;
				}
			}

			// restore column widths
			foreach (var column in dataGrid.Columns)
			{
				var header = column.Header?.ToString();
				if (header == null) continue;

				if (gridSettings.ColumnWidths.TryGetValue(header, out var width))
				{
					column.Width = new DataGridLength(width);
				}

				if (gridSettings.ColumnVisibility.TryGetValue(header, out var isVisible))
				{
					column.IsVisible = isVisible;
				}
			}
		}

		private static void SaveColumnSettings(DataGrid? dataGrid)
		{
			var settingsService = ServiceManager.TryGetService<ISettingsService>();

			if (settingsService is null || dataGrid?.Name is null) return;

			var settings = settingsService.Settings;

			if (!settings.DataGrids.TryGetValue(dataGrid.Name, out DataGridSettings? gridSettings))
			{
				gridSettings = new DataGridSettings();
				settings.DataGrids[dataGrid.Name] = gridSettings;
			}

			foreach (var column in dataGrid.Columns)
			{
				var header = column.Header?.ToString();
				if (header == null) continue;

				if (column.Width.IsAbsolute)
				{
					gridSettings.ColumnWidths[header] = column.Width.Value;
				}

				gridSettings.ColumnOrder[header] = column.DisplayIndex;
				gridSettings.ColumnVisibility[header] = column.IsVisible;
			}

			settingsService.Save();
		}
	}
}
