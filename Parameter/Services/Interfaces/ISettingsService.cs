using Parameter.Models;

namespace Parameter.Services.Interfaces
{
	public interface ISettingsService
	{
		public AppSettings Settings { get; }

		public void Load();

		public void Save();
	}
}
