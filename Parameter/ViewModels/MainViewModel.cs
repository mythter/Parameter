using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Myth.Avalonia.Services.Abstractions;
using Myth.Avalonia.Services.Extensions;

using Parameter.Entities.Enums;
using Parameter.Entities.Models;
using Parameter.Enums;
using Parameter.Helpers;
using Parameter.Models;
using Parameter.Services.Interfaces;

using ResourceNotFoundException = Amazon.SecretsManager.Model.ResourceNotFoundException;

namespace Parameter.ViewModels;

public partial class MainViewModel : ViewModelBase, IDialogContext
{
	#region Constants

	private const int DROPDOWN_HISTORY_MAX_COUNT = 50;

	#endregion

	#region Private Fields

	private bool _isLoading;

	private readonly IAppDataProvider<AppData> _appDataProvider;

	#endregion

	#region Public Properties

	public AppData AppData => _appDataProvider.Value;

	public SearchSettings SearchSettings => _appDataProvider.Value.SearchSettings;

	public AppSettings AppSettings => _appDataProvider.Value.AppSettings;

	[ObservableProperty]
	public partial string? CustomAccessKey { get; set; }

	[ObservableProperty]
	public partial string? CustomSecretKey { get; set; }

	[ObservableProperty]
	public partial RegionEndpoint? SelectedRegion { get; set; }

	public ObservableCollection<RegionEndpoint> Regions { get; } = new(RegionEndpoint.EnumerableAllRegions);

	[ObservableProperty]
	public partial ICollection<string> Profiles { get; set; } = [];

	[ObservableProperty]
	public partial string? PrefixText { get; set; }

	[ObservableProperty]
	public partial string? ParameterText { get; set; }

	public ObservableCollection<ParameterModel> Parameters { get; set; } = [];

	public bool HideAllParameters
	{
		get => Parameters is { Count: 0 } && field ||
			   Parameters is { Count: > 0 } && Parameters.All(p => p.Hidden);
		set
		{
			field = value;

			foreach (var p in Parameters)
				p.Hidden = value;

			OnPropertyChanged();
			OnPropertyChanged(nameof(Parameters));
		}
	}

	public static AwsCredentialsStorageLocation[] AwsCredentialsStorageLocations { get; } = Enum.GetValues<AwsCredentialsStorageLocation>();

	public static SearchSource[] SearchSources { get; } = Enum.GetValues<SearchSource>();

	#endregion

	#region Services

	private readonly IAwsProfilesService _awsProfilesService;

	private readonly IParameterServiceFactory _parameterServiceFactory;

	private readonly IPlatformServicesAccessor _platformServices;

	#endregion

	#region Constructors

	public MainViewModel()
	{
		AppData?.CredentialsFilePath = @"C:\Test\Path\.aws\credentials";

		Parameters.CollectionChanged += OnParametersCollectionChanged;

		Parameters.Add(new ParameterModel { Name = "Param 1", Value = "Value 1", Source = SearchSource.SSMParameterStore });
		Parameters.Add(new ParameterModel { Name = "Some param", Value = "Some value", Source = SearchSource.SSMParameterStore });
		Parameters.Add(new ParameterModel { Name = "Secret1", Value = "Secret value", Source = SearchSource.SecretsManager });
		Parameters.Add(new ParameterModel { Name = "Secret2", Value = "Secret value 2", Source = SearchSource.SecretsManager, Hidden = true });
	}

	public MainViewModel(
		IAwsProfilesService awsProfilesService,
		IParameterServiceFactory parameterServiceFactory,
		IPlatformServicesAccessor platformServices,
		IAppDataProvider<AppData> appDataProvider)
	{
		_awsProfilesService = awsProfilesService;
		_parameterServiceFactory = parameterServiceFactory;
		_platformServices = platformServices;
		_appDataProvider = appDataProvider;

		LoadData();

		SubscribeToChanges();
	}

	#endregion

	#region Commands

	[RelayCommand]
	private async Task SelectCredentialsFile()
	{
		var filePath = await this.ShowOpenFileDialogAsync("Choose AWS credentials file");

		if (filePath is not null)
		{
			AppData.CredentialsFilePath = filePath;
		}
	}

	[RelayCommand(IncludeCancelCommand = true)]
	private async Task SearchParameter(CancellationToken cancellationToken)
	{
		if (!await CanSearchParameter())
			return;

		Parameters.Clear();

		var creds = GetAwsProfileCredentials(AppData.SelectedAwsCredentialsLocation);

		var parameterPath = PrefixText + ParameterText;

		List<ParameterModel> parameters;

		try
		{
			if (SearchSettings.SelectedSearchSource == SearchSource.SSMParameterStore)
			{
				parameters = await GetSsmParameters(parameterPath, creds!, cancellationToken: cancellationToken);
			}
			else if (SearchSettings.SelectedSearchSource == SearchSource.SecretsManager)
			{
				parameters = await GetSecrets(parameterPath, creds!, cancellationToken: cancellationToken);
			}
			else // Everywhere
			{
				parameters = await GetSsmParameters(parameterPath, creds!, showErrorMessage: false, cancellationToken);
				var secrets = await GetSecrets(parameterPath, creds!, showErrorMessage: false, cancellationToken);
				parameters.AddRange(secrets);
			}
		}
		catch (OperationCanceledException)
		{
			// ignore
			return;
		}

		foreach (var p in parameters)
		{
			p.Hidden = HideAllParameters;
			Parameters.Add(p);
		}
	}

	[RelayCommand]
	private async Task CopyCell(DataGrid dataGrid)
	{
		if (dataGrid == null)
			return;

		var textToCopy = GetCellText(dataGrid);

		if (!string.IsNullOrEmpty(textToCopy) && _platformServices.Clipboard is { } clipboard)
		{
			await clipboard.SetTextAsync(textToCopy);
		}
	}

	[RelayCommand]
	private void AddToPrefixHistory(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		MoveToTop(text, AppData.PrefixHistory, DROPDOWN_HISTORY_MAX_COUNT);
	}

	[RelayCommand]
	private void AddToParameterHistory(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		MoveToTop(text, AppData.ParameterHistory, DROPDOWN_HISTORY_MAX_COUNT);
	}

	[RelayCommand]
	private void RemovePrefixFromHistory(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		RemoveFromCollection(text, AppData.PrefixHistory);
	}

	[RelayCommand]
	private void RemoveParameterFromHistory(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		RemoveFromCollection(text, AppData.ParameterHistory);
	}

	[RelayCommand]
	private void PrefixKeyDown(KeyEventArgs? e)
	{
		if (e?.Key != Key.Enter)
			return;

		AddToPrefixHistory(PrefixText);
	}

	[RelayCommand]
	private void ParameterKeyDown(KeyEventArgs? e)
	{
		if (e?.Key != Key.Enter)
			return;

		AddToParameterHistory(ParameterText);
	}

	#endregion

	#region Private Methods

	partial void OnProfilesChanged(ICollection<string> value)
	{
		if (_isLoading) return;

		Dispatcher.UIThread.Post(() =>
		{
			AppData.SelectedAwsProfile = value?.FirstOrDefault();
		});
	}

	partial void OnSelectedRegionChanged(RegionEndpoint? value)
	{
		AppData.SelectedRegion = value?.SystemName;
	}

	private void SubscribeToChanges()
	{
		Parameters.CollectionChanged += OnParametersCollectionChanged;
		AppData.PropertyChanged += (s, e) =>
		{
			if(e.PropertyName == nameof(AppData.SelectedAwsCredentialsLocation)
			|| e.PropertyName == nameof(AppData.CredentialsFilePath))
			{
				InitAwsProfiles();
			}
		};
	}

	private void OnParametersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems is not null)
		{
			foreach (ParameterModel item in e.NewItems)
				item.PropertyChanged += OnParameterPropertyChanged;
		}

		if (e.OldItems is not null)
		{
			foreach (ParameterModel item in e.OldItems)
				item.PropertyChanged -= OnParameterPropertyChanged;
		}

		OnPropertyChanged(nameof(HideAllParameters));
	}

	private void OnParameterPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ParameterModel.Hidden))
		{
			OnPropertyChanged(nameof(HideAllParameters));
		}
	}

	private void InitAwsProfiles()
	{
		if (!Design.IsDesignMode)
		{
			Profiles = GetAwsProfiles();
		}
	}

	private static string? GetCellText(DataGrid dataGrid)
	{
		var currentCell = dataGrid
			.FindDescendantOfType<DataGridRowsPresenter>()?
			.Children.OfType<DataGridRow>()
			.SelectMany(r => r
				.FindDescendantOfType<DataGridCellsPresenter>()
				?.Children.OfType<DataGridCell>() ?? [])
			.FirstOrDefault(c => c.Classes.Contains(":current"));

		if (currentCell == null)
			return string.Empty;

		// Value column might be masked
		if (string.Equals("value", dataGrid.CurrentColumn.Tag?.ToString(), StringComparison.OrdinalIgnoreCase))
		{
			var row = currentCell.FindAncestorOfType<DataGridRow>();

			return row?.DataContext is not ParameterModel parameter
				? string.Empty
				: parameter.Value;
		}

		var textBlock = currentCell.GetVisualDescendants()
			.OfType<TextBlock>()
			.FirstOrDefault();

		return textBlock?.Text ?? string.Empty;
	}

	private async Task<List<ParameterModel>> GetSsmParameters(string parameterPath, AWSCredentials creds, bool showErrorMessage = true, CancellationToken cancellationToken = default)
	{
		Task ShowErrorMessage(string message, string? title = null)
			=> showErrorMessage
				? this.ShowErrorMessageBoxDialog(message, title)
				: Task.CompletedTask;

		var ssm = _parameterServiceFactory.CreateSsmService(creds, SelectedRegion!);

		// this is a requirement for SSM parameter
		if (!parameterPath.StartsWith('/'))
		{
			parameterPath = $"/{parameterPath}";
		}

		try
		{
			var paramsByPath = await ssm.GetParameterByPathAsync(parameterPath, SearchSettings.RecursiveSearch, cancellationToken: cancellationToken);

			if (paramsByPath.Count == 0)
			{
				paramsByPath.Add(await ssm.GetParameterAsync(parameterPath, cancellationToken: cancellationToken));
			}

			return paramsByPath;
		}
		catch (ParameterNotFoundException)
		{
			await ShowErrorMessage($"Parameter {parameterPath} not found", $"Parameter not found");
		}
		catch (AmazonSimpleSystemsManagementException ex) when (ex.Message.Contains("security token"))
		{
			await ShowErrorMessage($"The credentials are invalid or you have selected the wrong region", $"Invalid Credentials");
		}
		catch (AmazonSimpleSystemsManagementException ex)
		{
			await ShowErrorMessage(ex.Message);
		}

		return [];
	}

	private async Task<List<ParameterModel>> GetSecrets(string secretPath, AWSCredentials creds, bool showErrorMessage = true, CancellationToken cancellationToken = default)
	{
		Task ShowErrorMessage(string message, string? title = null)
			=> showErrorMessage
				? this.ShowErrorMessageBoxDialog(message, title)
				: Task.CompletedTask;

		var secrets = _parameterServiceFactory.CreateSecretsService(creds, SelectedRegion!);

		try
		{
			var secretsByPath = await secrets.GetSecretsByPrefixAsync(secretPath, cancellationToken);

			if (secretsByPath.Count == 0)
			{
				secretsByPath.Add(await secrets.GetSecretAsync(secretPath, cancellationToken));
			}

			return secretsByPath;
		}
		catch (ResourceNotFoundException)
		{
			await ShowErrorMessage($"Secret {secretPath} not found", $"Secret not found");
		}
		catch (AmazonSecretsManagerException ex) when (ex.Message.Contains("security token"))
		{
			await ShowErrorMessage($"The credentials are invalid or you have selected the wrong region", $"Invalid Credentials");
		}
		catch (AmazonSecretsManagerException ex)
		{
			await ShowErrorMessage(ex.Message);
		}

		return [];
	}

	private async Task<bool> CanSearchParameter()
	{
		if (AppData.SelectedAwsCredentialsLocation == AwsCredentialsStorageLocation.Custom
			&& (string.IsNullOrWhiteSpace(CustomAccessKey) || string.IsNullOrWhiteSpace(CustomSecretKey)))
		{
			await this.ShowWarningMessageBoxDialog("Both custom access key and secret key must be provided.");
			return false;
		}

		if ((AppData.SelectedAwsCredentialsLocation is AwsCredentialsStorageLocation.SharedCredentialsFile or AwsCredentialsStorageLocation.NetEncryptedStore)
			&& AppData.SelectedAwsProfile is null)
		{
			await this.ShowWarningMessageBoxDialog("Select AWS profile to retrieve parameter for.");
			return false;
		}

		if (SelectedRegion is null)
		{
			await this.ShowWarningMessageBoxDialog("Select AWS region.");
			return false;
		}

		if (string.IsNullOrWhiteSpace(ParameterText))
		{
			await this.ShowWarningMessageBoxDialog("Enter valid paramter name or path.");
			return false;
		}

		return true;
	}

	private static void MoveToTop(string text, ObservableCollection<string> collection, int maxCount)
	{
		Dispatcher.UIThread.Post(() =>
		{
			var index = -1;

			for (int i = 0; i < collection.Count; i++)
			{
				if (string.Equals(collection[i], text, StringComparison.OrdinalIgnoreCase))
				{
					index = i;
					break;
				}
			}

			if (index > 0)
				collection.RemoveAt(index);

			if (collection.Count + 1 > maxCount)
			{
				for (int i = collection.Count - 1; i >= maxCount - 1; i--)
				{
					collection.RemoveAt(i);
				}
			}

			if (index != 0)
				collection.Insert(0, text);
		});
	}

	private static void RemoveFromCollection(string item, ObservableCollection<string> collection)
	{
		Dispatcher.UIThread.Post(() =>
		{
			collection.Remove(item);
		});
	}

	private ObservableCollection<string> GetAwsProfiles()
	{
		var profiles = AppData.SelectedAwsCredentialsLocation switch
		{
			AwsCredentialsStorageLocation.SharedCredentialsFile => _awsProfilesService.GetAllSharedProfiles(AppData.CredentialsFilePath),
			AwsCredentialsStorageLocation.NetEncryptedStore => _awsProfilesService.GetAllNetEncryptedProfiles(),
			_ => null
		};

		return new ObservableCollection<string>(profiles ?? []);
	}

	private AWSCredentials? GetAwsProfileCredentials(AwsCredentialsStorageLocation storageLocation)
	{
		return storageLocation switch
		{
			AwsCredentialsStorageLocation.SharedCredentialsFile => _awsProfilesService.GetSharedProfileCredentials(AppData.SelectedAwsProfile!, AppData.CredentialsFilePath),
			AwsCredentialsStorageLocation.NetEncryptedStore => _awsProfilesService.GetNetEncryptedCredentials(AppData.SelectedAwsProfile!),
			AwsCredentialsStorageLocation.Custom => new BasicAWSCredentials(CustomAccessKey, CustomSecretKey),
			_ => throw new NotImplementedException()
		};
	}

	private void LoadData()
	{
		_isLoading = true;

		try
		{
			AppData.CredentialsFilePath ??= _awsProfilesService.GetDefaultCredentialsPath();

			Profiles = GetAwsProfiles();
			AppData.SelectedAwsProfile = Profiles.FirstOrDefault(p => p == AppData.SelectedAwsProfile) ?? Profiles.FirstOrDefault();

			SelectedRegion = Regions.FirstOrDefault(r => r.SystemName == AppData.SelectedRegion);
		}
		finally
		{
			_isLoading = false;
		}
	}

	#endregion
}
