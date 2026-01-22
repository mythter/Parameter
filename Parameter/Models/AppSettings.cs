using System.Collections.Generic;

namespace Parameter.Models
{
	public class AppSettings
	{
		public WindowSettings Window { get; set; } = new();

		public Dictionary<string, DataGridSettings> DataGrids { get; set; } = [];

		public AppData Data { get; set; } = new();
	}
}
