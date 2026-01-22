using System.Collections.Generic;

using Parameter.Entites.Enums;
using Parameter.Entites.Models;
using Parameter.Enums;

namespace Parameter.Models
{
	public class AppData
	{
		public List<string> ParameterHistory { get; set; } = [];

		public List<string> PrefixHistory { get; set; } = [];

		public string? CredentialsFilePath { get; set; }

		public string? SelectedAwsProfile { get; set; }

		public AwsCredentialsStorageLocation? SelectedAwsCredentialsLocation { get; set; }

		public string? SelectedRegion { get; set; }

		public bool? HideAllParameters { get; set; }

		public SearchSource? SelectedSearchSource { get; set; }
	}
}
