using CommunityToolkit.Mvvm.ComponentModel;

namespace Parameter.Models;

public partial class AppSettings : ObservableObject
{
	public DataGridSettings DataGridSettings { get; set; } = new();

	[ObservableProperty]
	public partial bool HideAllParameters { get; set; } = false;

	[ObservableProperty]
	public partial bool IsAwsCredentialsExpanded { get; set; } = true;
}
