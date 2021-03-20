using TimeTracker.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace TimeTracker.Services
{
    public class SettingsService : ISettingsService
    {
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaSch", "TimeTracker");
        private static readonly string SettingFilePath = Path.Combine(AppDataPath, "settings.json");
        private static readonly string GuiSettingsFilePath = Path.Combine(AppDataPath, "settings.gui.json");

        public GuiSettings LoadGuiSettings()
        {
            GuiSettings result;

            if (!File.Exists(GuiSettingsFilePath))
                result = new GuiSettings();
            else
                result = JsonConvert.DeserializeObject<GuiSettings>(File.ReadAllText(GuiSettingsFilePath));

            return result;
        }

        public void SaveGuiSettings(GuiSettings settings)
        {
            Directory.CreateDirectory(AppDataPath);
            File.WriteAllText(GuiSettingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
    }
}
