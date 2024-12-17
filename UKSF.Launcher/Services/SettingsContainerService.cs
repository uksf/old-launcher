using UKSF.Launcher.Models;

namespace UKSF.Launcher.Services {
    public class SettingsContainerService : ISettingsContainerService {
        public SettingsContainer SettingsContainer = new SettingsContainer();
        public ConstantsContainer ConstantsContainer = new ConstantsContainer();
    }
}
