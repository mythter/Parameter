using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

using Parameter.Models;
using Parameter.Services;
using Parameter.Services.Interfaces;

namespace Parameter.Behaviors;

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
		var appDataProvider = ServiceManager.TryGetService<IAppDataProvider<AppData>>();

		if (appDataProvider is null || dataGrid is null) return;

		var settings = appDataProvider.Value.AppSettings.DataGridSettings;

		if (settings is null)
			return;

		// restore column order
		foreach (var column in dataGrid.Columns)
		{
			var header = column.Header?.ToString();
			if (header == null) continue;

			if (settings.ColumnOrder.TryGetValue(header, out var displayIndex))
			{
				column.DisplayIndex = displayIndex;
			}
		}

		// restore column widths
		foreach (var column in dataGrid.Columns)
		{
			var header = column.Header?.ToString();
			if (header == null) continue;

			if (settings.ColumnWidths.TryGetValue(header, out var width))
			{
				column.Width = new DataGridLength(width);
			}

			if (settings.ColumnVisibility.TryGetValue(header, out var isVisible))
			{
				column.IsVisible = isVisible;
			}
		}
	}

	private static void SaveColumnSettings(DataGrid? dataGrid)
	{
		var appDataProvider = ServiceManager.TryGetService<IAppDataProvider<AppData>>();

		if (appDataProvider is null || dataGrid is null) return;

		var settings = appDataProvider.Value.AppSettings.DataGridSettings ?? new DataGridSettings();

		if (appDataProvider.Value.AppSettings.DataGridSettings is null)
		{
			appDataProvider.Value.AppSettings.DataGridSettings = settings;
		}

		foreach (var column in dataGrid.Columns)
		{
			var header = column.Header?.ToString();
			if (header == null) continue;

			if (column.Width.IsAbsolute)
			{
				settings.ColumnWidths[header] = column.Width.Value;
			}

			settings.ColumnOrder[header] = column.DisplayIndex;
			settings.ColumnVisibility[header] = column.IsVisible;
		}

		appDataProvider.Save();
	}
}
