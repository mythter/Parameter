using System.Text.Json.Serialization;

using Parameter.Models;

namespace Parameter
{
	[JsonSerializable(typeof(AppSettings))]
	[JsonSerializable(typeof(AppData))]
	[JsonSerializable(typeof(WindowSettings))]
	[JsonSerializable(typeof(DataGridSettings))]
	[JsonSourceGenerationOptions(
		WriteIndented = true,
		PropertyNameCaseInsensitive = true)]
	public partial class AppJsonContext : JsonSerializerContext
	{
	}
}
