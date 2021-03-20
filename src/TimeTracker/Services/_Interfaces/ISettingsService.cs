using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface ISettingsService
    {
        GuiSettings LoadGuiSettings();
        void SaveGuiSettings(GuiSettings settings);
    }
}
