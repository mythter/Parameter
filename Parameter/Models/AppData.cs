using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Parameter.Enums;

namespace Parameter.Models;

public partial class AppData : ObservableObject
{
	public ObservableCollection<string> ParameterHistory { get; set; } = [];

	public ObservableCollection<string> PrefixHistory { get; set; } = [];

	[ObservableProperty]
	public partial string? CredentialsFilePath { get; set; }

	[ObservableProperty]
	public partial string? SelectedAwsProfile { get; set; }

	[ObservableProperty]
	public partial AwsCredentialsStorageLocation SelectedAwsCredentialsLocation { get; set; } = AwsCredentialsStorageLocation.SharedCredentialsFile;

	[ObservableProperty]
	public partial string? SelectedRegion { get; set; }

	public SearchSettings SearchSettings { get; set; } = new();

	public WindowSettings WindowSettings { get; set; } = new();

	public AppSettings AppSettings { get; set; } = new();
}
