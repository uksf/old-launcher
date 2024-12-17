using System;

namespace UKSF.Launcher.Services {
    public class StartupService : IStartupService {
        private readonly ISettingsContainerService _settingsContainerService;

        public StartupService(ISettingsContainerService settingsContainerService) {
            _settingsContainerService = settingsContainerService;
        }
        
        public void Start(bool updated) {
#if DEBUG
            //_settingsContainerService.
            Console.Out.WriteLine("Brexit");
#endif
        }
    }
}
