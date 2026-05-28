using Avalonia.Controls;

using Parameter.Models;
using Parameter.Services.Interfaces;

namespace Parameter.Views;

public partial class MainWindow : Window
{
	#region Private Fields

	private readonly IAppDataProvider<AppData> _appDataProvider;

	private readonly IWindowStateService _windowStateService;

	#endregion

	#region Constructors

	public MainWindow(IAppDataProvider<AppData> appDataProvider, IWindowStateService windowStateService)
	{
		_appDataProvider = appDataProvider;
		_windowStateService = windowStateService;

		InitializeComponent();

		_windowStateService.Restore(this, _appDataProvider.Value.WindowSettings);
	}

	#endregion

	#region Overrides

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		base.OnClosing(e);

		_windowStateService.Save(this, _appDataProvider.Value.WindowSettings);
		_appDataProvider.Save();
	}

	#endregion
}
