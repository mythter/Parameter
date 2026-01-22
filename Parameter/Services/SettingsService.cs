using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Parameter.Models;
using Parameter.Services.Interfaces;

namespace Parameter.Services
{
	public class SettingsService(IDialogService dialogService) : ISettingsService
	{
		private static readonly string AppDataFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			nameof(Parameter));

		private static readonly string settingsFilePath = Path.Combine(AppDataFolder, "settings.json");

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		};

		private AppSettings? _currentSettings;

		private readonly Lock _lock = new();

		public AppSettings Settings
		{
			get
			{
				lock (_lock)
				{
					_currentSettings ??= new AppSettings();
					return _currentSettings;
				}
			}
		}

		public async Task LoadAsync()
		{
			if (!File.Exists(settingsFilePath))
			{
				_currentSettings = new AppSettings();
				return;
			}

			var json = await File.ReadAllTextAsync(settingsFilePath);
			_currentSettings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
		}

		public void Load()
		{
			if (!File.Exists(settingsFilePath))
			{
				_currentSettings = new AppSettings();
				return;
			}

			var json = File.ReadAllText(settingsFilePath);
			_currentSettings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
		}

		public async Task SaveAsync()
		{
			try
			{
				Directory.CreateDirectory(AppDataFolder);

				var json = JsonSerializer.Serialize(Settings, JsonOptions);
				await File.WriteAllTextAsync(settingsFilePath, json);
			}
			catch (Exception ex)
			{
				await dialogService.ShowErrorAsync($"Error while saving settings: {ex.Message}");
			}
		}

		public void Save()
		{
			try
			{
				Directory.CreateDirectory(AppDataFolder);

				var json = JsonSerializer.Serialize(Settings, JsonOptions);
				File.WriteAllText(settingsFilePath, json);
			}
			catch (Exception ex)
			{
				dialogService.ShowErrorAsync($"Error while saving settings: {ex.Message}").GetAwaiter().GetResult();
			}
		}
	}
}
