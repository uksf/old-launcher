using Microsoft.Win32;

namespace UKSF.Old.Launcher.Utility {
    public class SettingsHandler {
        private readonly string _registry;

        public SettingsHandler(string registryKey) => _registry = registryKey;

        private RegistryKey GetRegistryKey() => Registry.CurrentUser.OpenSubKey(_registry, true) ?? Registry.CurrentUser.CreateSubKey(_registry, true);

        public string ReadSetting(string name, object defaultValue = null, bool silent = false) {
            RegistryKey registry = GetRegistryKey();
            object value = registry.GetValue(name);
            if (value == null && defaultValue != null) {
                value = defaultValue;
                WriteSetting(name, value, silent);
            }

            LogHandler.LogInfo($"Reading setting '{name}'{(silent ? "" : $": {value}")}");
            return value?.ToString();
        }

        public object WriteSetting(string name, object value, bool silent = false) {
            RegistryKey registry = GetRegistryKey();
            LogHandler.LogInfo($"Writing setting '{name}'{(silent ? "" : $": {value}")}");
            registry.SetValue(name, value);
            return value;
        }

        public string ParseSetting(string name, string defaultValue, bool silent = false) => ReadSetting(name, defaultValue, silent);

        public int ParseSetting(string name, int defaultValue, bool silent = false) => int.Parse(ReadSetting(name, defaultValue, silent));

        public bool ParseSetting(string name, bool defaultValue, bool silent = false) => bool.Parse(ReadSetting(name, defaultValue, silent));

        public void DeleteSetting(string name) {
            RegistryKey registry = GetRegistryKey();
            if (registry.GetValue(name) != null) {
                registry.DeleteValue(name);
            }
        }

        public void ResetSettings() => Registry.CurrentUser.DeleteSubKey(_registry);
    }
}
