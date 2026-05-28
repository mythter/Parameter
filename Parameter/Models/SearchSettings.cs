using CommunityToolkit.Mvvm.ComponentModel;

using Parameter.Entities.Enums;

namespace Parameter.Models;

public partial class SearchSettings : ObservableObject
{
	[ObservableProperty]
	public partial bool RecursiveSearch { get; set; } = true;

	[ObservableProperty]
	public partial SearchSource SelectedSearchSource { get; set; } = SearchSource.Everywhere;
}
