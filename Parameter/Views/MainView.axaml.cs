using Avalonia.Controls;
using Avalonia.Input;

namespace Parameter.Views;

public partial class MainView : UserControl
{
	public MainView()
	{
		InitializeComponent();
	}

	private async void OnDataGridKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.C && e.KeyModifiers == KeyModifiers.Control
			&& sender is DataGrid dataGrid && dataGrid.SelectedItem is TextBlock textBlock
			&& TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
		{
			await clipboard.SetTextAsync(textBlock.Text);
		}
	}
}
