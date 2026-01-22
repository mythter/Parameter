using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Parameter.Entites.Enums;
using Parameter.Entites.Models;
using Parameter.Enums;
using Parameter.Services.Interfaces;

using ResourceNotFoundException = Amazon.SecretsManager.Model.ResourceNotFoundException;

namespace Parameter.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	#region Private Fields

	private bool _isLoading;

	#endregion

	#region Observables

	[ObservableProperty]
	private bool _isAwsCredentialsExpanded;

	[ObservableProperty]
	private string? _credentialsFilePath;

	[ObservableProperty]
	private string? _customAccessKey;

	[ObservableProperty]
	private string? _customSecretKey;

	[ObservableProperty]
	private ParameterModel? _selectedParameter;

	[ObservableProperty]
	private RegionEndpoint? _selectedRegion;

	public ObservableCollection<RegionEndpoint> Regions { get; } = new(RegionEndpoint.EnumerableAllRegions);

	[ObservableProperty]
	private string? _selectedProfile;

	[ObservableProperty]
	private ICollection<string> _profiles = [];

	[ObservableProperty]
	private string? _prefixText;

	public ObservableCollection<string> PrefixHistory { get; set; } = [];

	[ObservableProperty]
	private string? _parameterText;

	public ObservableCollection<string> ParameterHistory { get; set; } = [];

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HideAllParameters))]
	private ObservableCollection<ParameterModel> _parameters = [];

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

	[ObservableProperty]
	private AwsCredentialsStorageLocation _selectedAwsCredentialsLocation = AwsCredentialsStorageLocation.SharedCredentialsFile;

	public static SearchSource[] SearchSources { get; } = Enum.GetValues<SearchSource>();

	[ObservableProperty]
	private SearchSource _selectedSearchSource = SearchSource.Everywhere;

	#endregion

	#region Services

	private readonly IDialogService _dialogService;

	private readonly IAwsProfilesService _awsProfilesService;

	private readonly IParameterServiceFactory _parameterServiceFactory;

	private readonly IPlatformServicesAccessor _platformServices;

	private readonly ISettingsService _settingsService;

	#endregion

	#region Constructors

	public MainViewModel()
	{
		CredentialsFilePath = @"C:\Test\Path\.aws\credentials";

		Parameters.CollectionChanged += OnParametersCollectionChanged;

		Parameters.Add(new ParameterModel { Name = "Param 1", Value = "Value 1", Source = SearchSource.SSMParameterStore });
		Parameters.Add(new ParameterModel { Name = "Some param", Value = "Some value", Source = SearchSource.SSMParameterStore });
		Parameters.Add(new ParameterModel { Name = "Secret1", Value = "Secret value", Source = SearchSource.SecretsManager });
		Parameters.Add(new ParameterModel { Name = "Secret2", Value = "Secret value 2", Source = SearchSource.SecretsManager, Hidden = true });
	}

	public MainViewModel(
		IDialogService dialogService,
		IAwsProfilesService awsProfilesService,
		IParameterServiceFactory parameterServiceFactory,
		IPlatformServicesAccessor platformServices,
		ISettingsService settingsService)
	{
		_dialogService = dialogService;
		_awsProfilesService = awsProfilesService;
		_parameterServiceFactory = parameterServiceFactory;
		_platformServices = platformServices;
		_settingsService = settingsService;

		LoadData();

		SubscribeToChanges();
	}

	#endregion

	#region Commands

	[RelayCommand]
	private async Task SelectCredentialsFile()
	{
		var file = await _dialogService.ShowAwsCredentialsFileDialogAsync();

		if (file is not null)
		{
			CredentialsFilePath = file.Path.AbsolutePath;
		}
	}

	[RelayCommand(IncludeCancelCommand = true)]
	private async Task SearchParameter(CancellationToken cancellationToken)
	{
		if (!await CanSearchParameter())
			return;

		Parameters.Clear();

		var creds = GetAwsProfileCredentials(SelectedAwsCredentialsLocation);

		var parameterPath = PrefixText + ParameterText;

		List<ParameterModel> parameters;

		try
		{
			if (SelectedSearchSource == SearchSource.SSMParameterStore)
			{
				parameters = await GetSsmParameters(parameterPath, creds!, cancellationToken: cancellationToken);
			}
			else if (SelectedSearchSource == SearchSource.SecretsManager)
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

		MoveToTop(text, PrefixHistory);
	}

	[RelayCommand]
	private void AddToParameterHistory(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		MoveToTop(text, ParameterHistory);
	}

	[RelayCommand]
	private void RemovePrefixFromHistory(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		RemoveFromCollection(text, PrefixHistory);
	}

	[RelayCommand]
	private void RemoveParameterFromHistory(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		RemoveFromCollection(text, ParameterHistory);
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

	partial void OnSelectedAwsCredentialsLocationChanged(AwsCredentialsStorageLocation value) => InitAwsProfiles();

	partial void OnCredentialsFilePathChanged(string? value) => InitAwsProfiles();

	partial void OnProfilesChanged(ICollection<string> value)
	{
		if (_isLoading) return;

		Dispatcher.UIThread.Post(() =>
		{
			SelectedProfile = value?.FirstOrDefault();
		});
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

	private static string GetCellText(DataGrid dataGrid)
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
				? _dialogService.ShowErrorAsync(message, title)
				: Task.CompletedTask;

		var ssm = _parameterServiceFactory.CreateSsmService(creds, SelectedRegion!);

		// this is a requirement for SSM parameter
		if (!parameterPath.StartsWith('/'))
		{
			parameterPath = $"/{parameterPath}";
		}

		try
		{
			var paramsByPath = await ssm.GetParameterByPathAsync(parameterPath, cancellationToken: cancellationToken);

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
				? _dialogService.ShowErrorAsync(message, title)
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
		if (SelectedAwsCredentialsLocation == AwsCredentialsStorageLocation.Custom
			&& (string.IsNullOrWhiteSpace(CustomAccessKey) || string.IsNullOrWhiteSpace(CustomSecretKey)))
		{
			await _dialogService.ShowWarningAsync("Both custom access key and secret key must be provided.");
			return false;
		}

		if ((SelectedAwsCredentialsLocation is AwsCredentialsStorageLocation.SharedCredentialsFile or AwsCredentialsStorageLocation.NetEncryptedStore)
			&& SelectedProfile is null)
		{
			await _dialogService.ShowWarningAsync("Select AWS profile to retrieve parameter for.");
			return false;
		}

		if (SelectedRegion is null)
		{
			await _dialogService.ShowWarningAsync("Select AWS region.");
			return false;
		}

		if (string.IsNullOrWhiteSpace(ParameterText))
		{
			await _dialogService.ShowWarningAsync("Enter valid paramter name or path.");
			return false;
		}

		return true;
	}

	private static void MoveToTop(string text, IList<string> collection)
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

			if (index != 0)
				collection.Insert(0, text);
		});
	}
	private static void RemoveFromCollection(string item, IList<string> collection)
	{
		Dispatcher.UIThread.Post(() =>
		{
			collection.Remove(item);
		});
	}

	private ObservableCollection<string> GetAwsProfiles()
	{
		var profiles = SelectedAwsCredentialsLocation switch
		{
			AwsCredentialsStorageLocation.SharedCredentialsFile => _awsProfilesService.GetAllSharedProfiles(CredentialsFilePath),
			AwsCredentialsStorageLocation.NetEncryptedStore => _awsProfilesService.GetAllNetEncryptedProfiles(),
			_ => null
		};

		return new ObservableCollection<string>(profiles ?? []);
	}

	private AWSCredentials? GetAwsProfileCredentials(AwsCredentialsStorageLocation storageLocation)
	{
		return storageLocation switch
		{
			AwsCredentialsStorageLocation.SharedCredentialsFile => _awsProfilesService.GetSharedProfileCredentials(SelectedProfile!, CredentialsFilePath),
			AwsCredentialsStorageLocation.NetEncryptedStore => _awsProfilesService.GetNetEncryptedCredentials(SelectedProfile!),
			AwsCredentialsStorageLocation.Custom => new BasicAWSCredentials(CustomAccessKey, CustomSecretKey),
			_ => throw new NotImplementedException()
		};
	}

	private void LoadData()
	{
		_isLoading = true;

		try
		{
			IsAwsCredentialsExpanded = _settingsService.Settings.Data.IsAwsCredentialsExpanded ?? true;
			HideAllParameters = _settingsService.Settings.Data.HideAllParameters ?? false;
			SelectedSearchSource = _settingsService.Settings.Data.SelectedSearchSource ?? SearchSource.Everywhere;
			CredentialsFilePath = _settingsService.Settings.Data.CredentialsFilePath ?? _awsProfilesService.GetDefaultCredentialsPath();
			SelectedAwsCredentialsLocation = _settingsService.Settings.Data.SelectedAwsCredentialsLocation ?? AwsCredentialsStorageLocation.SharedCredentialsFile;

			Profiles = GetAwsProfiles();

			SelectedProfile = Profiles.FirstOrDefault(p => p == _settingsService.Settings.Data.SelectedAwsProfile) ?? Profiles.FirstOrDefault();

			SelectedRegion = Regions.FirstOrDefault(r => r.SystemName == _settingsService.Settings.Data.SelectedRegion);

			foreach (var prefix in _settingsService.Settings.Data.PrefixHistory)
			{
				PrefixHistory.Add(prefix);
			}

			foreach (var param in _settingsService.Settings.Data.ParameterHistory)
			{
				ParameterHistory.Add(param);
			}
		}
		finally
		{
			_isLoading = false;
		}
	}

	private void SubscribeToChanges()
	{
		PropertyChanged += OnPropertyChanged;

		Parameters.CollectionChanged += OnCollectionChanged;
		Parameters.CollectionChanged += OnParametersCollectionChanged;

		PrefixHistory.CollectionChanged += OnCollectionChanged;
		ParameterHistory.CollectionChanged += OnCollectionChanged;
	}

	private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (_isLoading) return;

		SaveSettings();
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (_isLoading) return;

		var propertiesToSave = new[]
		{
			nameof(IsAwsCredentialsExpanded),
			nameof(HideAllParameters),
			nameof(SelectedSearchSource),
			nameof(CredentialsFilePath),
			nameof(SelectedAwsCredentialsLocation),
			nameof(SelectedProfile),
			nameof(SelectedRegion),
			nameof(PrefixText),
			nameof(ParameterText)
		};

		if (propertiesToSave.Contains(e.PropertyName))
		{
			SaveSettings();
		}
	}

	private void SaveSettings()
	{
		var settings = _settingsService.Settings.Data;

		settings.IsAwsCredentialsExpanded = IsAwsCredentialsExpanded;
		settings.HideAllParameters = HideAllParameters;
		settings.SelectedSearchSource = SelectedSearchSource;
		settings.CredentialsFilePath = CredentialsFilePath;
		settings.SelectedAwsCredentialsLocation = SelectedAwsCredentialsLocation;

		settings.SelectedAwsProfile = SelectedProfile;
		settings.SelectedRegion = SelectedRegion?.SystemName;

		settings.PrefixHistory = [.. PrefixHistory];
		settings.ParameterHistory = [.. ParameterHistory];

		_settingsService.Save();
	}

	#endregion
}
