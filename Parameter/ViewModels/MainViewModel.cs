using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Parameter.Enums;
using Parameter.Services.Implementations;

namespace Parameter.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	private Lock _lock = new();

	[ObservableProperty]
	private string? _credentialsFilePath;

	[ObservableProperty]
	private string? _customAccessKey;

	[ObservableProperty]
	private string? _customSecretKey;

	public ObservableCollection<Person> People { get; }

	[ObservableProperty]
	private Product? _selectedProduct;

	[ObservableProperty]
	private RegionEndpoint _selectedRegion;

	public ObservableCollection<RegionEndpoint> Regions { get; } = new(RegionEndpoint.EnumerableAllRegions);

	[ObservableProperty]
	private string? _selectedProfile;

	public ObservableCollection<string> Profiles => GetAwsProfiles();

	[ObservableProperty]
	private string? _parameterText;

	[ObservableProperty]
	private string? _parameter;

	public ObservableCollection<string> ParameterHistory { get; } = [];


	public ObservableCollection<Product> Products { get; } =
	[
		new Product { Id = 1, Name = "Laptop", Price = 999.99m },
		new Product { Id = 2, Name = "Mouse", Price = 29.99m },
		new Product { Id = 3, Name = "Keyboard", Price = 79.99m },
		new Product { Id = 4, Name = "Monitor", Price = 299.99m },
		new Product { Id = 5, Name = "Headphones", Price = 149.99m }
	];

	public static IEnumerable<AwsCredentialsStorageLocation> AwsCredentialsStorageLocations { get; } = Enum.GetValues<AwsCredentialsStorageLocation>();

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Profiles))]
	private AwsCredentialsStorageLocation _selectedAwsCredentialsLocation = AwsCredentialsStorageLocation.SharedCredentialsFile;

	public static IEnumerable<SearchSource> SearchSources { get; } = Enum.GetValues<SearchSource>();

	[ObservableProperty]
	private SearchSource _selectedSearchSource = SearchSource.Everywhere;

	public MainViewModel()
	{
		var reg = RegionEndpoint.EnumerableAllRegions.First();

		var awsProfilesService = new AwsProfilesService();

		var prof = awsProfilesService.GetAllSharedProfiles();

		CredentialsFilePath = awsProfilesService.GetDefaultCredentialsPath();

		var people = new List<Person>
			{
				new Person("Neil", "Armstrong"),
				new Person("Buzz", "Lightyear"),
				new Person("James", "Kirk")
			};

		People = new ObservableCollection<Person>(people);
	}

	[RelayCommand]
	public async Task SearchParameter()
	{
		if (SelectedAwsCredentialsLocation == AwsCredentialsStorageLocation.Custom && (CustomAccessKey is null || CustomSecretKey is null))
			return;

		if (SelectedProfile is null || SelectedRegion is null)
			return;

		var awsProfilesService = new AwsProfilesService();

		var creds = SelectedAwsCredentialsLocation switch
		{
			AwsCredentialsStorageLocation.SharedCredentialsFile => awsProfilesService.GetSharedProfileCredentials(SelectedProfile, CredentialsFilePath),
			AwsCredentialsStorageLocation.NetEncryptedStore => awsProfilesService.GetNetEncryptedCredentials(SelectedProfile),
			AwsCredentialsStorageLocation.Custom => new BasicAWSCredentials(CustomAccessKey, CustomSecretKey),
			_ => throw new NotImplementedException()
		};

		var ssm = new SsmService(creds!, SelectedRegion);
		var sec = new SecretsService(creds!, SelectedRegion);

	}

	[RelayCommand]
	public void AddToHistory(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		MoveToTop(text);
	}

	[RelayCommand]
	public void OnKeyDown(KeyEventArgs? e)
	{
		if (e?.Key != Key.Enter)
			return;

		// текст можно взять из биндинга SearchText
		AddToHistory(ParameterText);
	}

	private void MoveToTop(string text)
	{
		Dispatcher.UIThread.Post(() =>
		{
			var index = -1;

			for (int i = 0; i < ParameterHistory.Count; i++)
			{
				if (string.Equals(ParameterHistory[i], text, StringComparison.OrdinalIgnoreCase))
				{
					index = i;
					break;
				}
			}

			if (index > 0)
				ParameterHistory.RemoveAt(index);

			if (index != 0)
				ParameterHistory.Insert(0, text);
		});
	}

	private ObservableCollection<string> GetAwsProfiles()
	{
		var awsProfilesService = new AwsProfilesService();

		var profiles = SelectedAwsCredentialsLocation switch
		{
			AwsCredentialsStorageLocation.SharedCredentialsFile => awsProfilesService.GetAllSharedProfiles(CredentialsFilePath),
			AwsCredentialsStorageLocation.NetEncryptedStore => awsProfilesService.GetAllNetEncryptedProfiles(),
			_ => null
		};

		SelectedProfile = profiles?.FirstOrDefault();

		return new ObservableCollection<string>(profiles ?? []);
	}

	private AWSCredentials? GetAwsProfileCredentials(string profile, AwsCredentialsStorageLocation storageLocation)
	{
		var awsProfilesService = new AwsProfilesService();
		var credentials = null as AWSCredentials;

		if (storageLocation == AwsCredentialsStorageLocation.SharedCredentialsFile)
		{
			credentials = awsProfilesService.GetSharedProfileCredentials(profile, CredentialsFilePath);
		}
		else if (storageLocation == AwsCredentialsStorageLocation.NetEncryptedStore)
		{
			credentials = awsProfilesService.GetNetEncryptedCredentials(profile);
		}
		else if (CustomAccessKey is not null && CustomSecretKey is not null)
		{
			credentials = new BasicAWSCredentials(CustomAccessKey, CustomSecretKey);
		}

		return credentials;
	}
}

public class Person
{
	public string FirstName { get; set; }
	public string LastName { get; set; }

	public Person(string firstName, string lastName)
	{
		FirstName = firstName;
		LastName = lastName;
	}
}

public class Product : INotifyPropertyChanged
{
	private int id;
	private string name = string.Empty;
	private decimal price;

	public int Id
	{
		get => id;
		set
		{
			if (id != value)
			{
				id = value;
				OnPropertyChanged(nameof(Id));
			}
		}
	}

	public string Name
	{
		get => name;
		set
		{
			if (name != value)
			{
				name = value;
				OnPropertyChanged(nameof(Name));
			}
		}
	}

	public decimal Price
	{
		get => price;
		set
		{
			if (price != value)
			{
				price = value;
				OnPropertyChanged(nameof(Price));
			}
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged(string propertyName) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
