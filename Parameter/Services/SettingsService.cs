using System;
using System.IO;
using System.Text.Json;
using System.Threading;

using Parameter.Models;
using Parameter.Services.Interfaces;

namespace Parameter.Services
{
	public class SettingsService : ISettingsService
	{
		private static readonly string AppDataFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			nameof(Parameter));

		private static readonly string _settingsFilePath = Path.Combine(AppDataFolder, "settings.json");

		private AppSettings? _currentSettings;

		private readonly Lock _fileLock = new();

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

		public void Load()
		{
			if (!File.Exists(_settingsFilePath))
			{
				_currentSettings = new AppSettings();
				return;
			}

			string json;

			lock (_fileLock)
			{
				json = File.ReadAllText(_settingsFilePath);
			}

			_currentSettings = JsonSerializer.Deserialize(json, AppJsonContext.Default.AppSettings) ?? new AppSettings();
		}

		public void Save()
		{
			Directory.CreateDirectory(AppDataFolder);

			var json = JsonSerializer.Serialize(Settings, AppJsonContext.Default.AppSettings);

			lock (_fileLock)
			{
				File.WriteAllText(_settingsFilePath, json);
			}
		}
	}
}
