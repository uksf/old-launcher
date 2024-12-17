namespace UKSF.Launcher.Models {
    public class ConstantsContainer {
        public const string FORMAT_DATE = "yyyy-MM-dd__HH-mm-ss";
        public const string FORMAT_TIME = "HH:mm:ss";
        public const string GAME_REGISTRY = @"SOFTWARE\WOW6432Node\bohemia interactive\arma 3";
        public const string MALLOC_SYSTEM_DEFAULT = "System Default";
        public const string REGSITRY = @"SOFTWARE\UKSF Launcher";
        public const long REQUIREDSPACE = 32212254720; // ~30GB // TODO: Get modpack size from api
    }
}
