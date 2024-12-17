namespace UKSF.Launcher.Network {
    public class Global {
#if DEBUG
        public const string URL = "http://localhost:5000";
#else
        public const string URL = "https://api.uk-sf.co.uk";
#endif
    }
}
