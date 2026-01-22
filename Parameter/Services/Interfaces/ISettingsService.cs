using System.Threading.Tasks;

using Parameter.Models;

namespace Parameter.Services.Interfaces
{
	public interface ISettingsService
	{
		public AppSettings Settings { get; }

		public Task LoadAsync();

		public void Load();

		public Task SaveAsync();

		public void Save();
	}
}
