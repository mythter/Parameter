using System.Collections.Generic;

namespace Parameter.Models
{
	public class DataGridSettings
	{
		/// <summary>
		/// Column widths (key - DisplayIndex or column name)
		/// </summary>
		public Dictionary<string, double> ColumnWidths { get; set; } = [];

		/// <summary>
		/// Column order (key - column name, value - DisplayIndex)
		/// </summary>
		public Dictionary<string, int> ColumnOrder { get; set; } = [];

		/// <summary>
		/// Column visibility (key - column name, value - isVisible)
		/// </summary>
		public Dictionary<string, bool> ColumnVisibility { get; set; } = [];
	}
}
