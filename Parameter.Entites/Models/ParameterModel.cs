using System.ComponentModel;

using Parameter.Entities.Enums;

namespace Parameter.Entities.Models
{
	public class ParameterModel : INotifyPropertyChanged
	{
		public required string Name { get; set; }

		public string Value { get; set; }

		public SearchSource Source { get; set; }

		public bool Hidden
		{
			get => field;
			set
			{
				if (field != value)
				{
					field = value;
					OnPropertyChanged(nameof(Hidden));
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged(string propertyName) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
