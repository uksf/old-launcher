using System.IO;
using System.Windows;
using UKSF.Old.Launcher.Game;
using UKSF.Old.Launcher.UI.General;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher.UI.FTS {
    public partial class FtsModLocationControl {
        private const string TITLE = "Mod Location";

        private static readonly string DESCRIPTION = "We have selected your Arma 3 install location as your mod download location." + Global.Constants.NL +
                                                     "If you wish to change this, select the folder below.";

        private static readonly string DESCRIPTION_NOINSTALL = "We can't find your Arma 3 installation." + Global.Constants.NL +
                                                               "This is unusual, so you should check the game is installed in Steam." + Global.Constants.NL +
                                                               "You can continue by selecting the mod download location you wish to use manually. (Not recommended)";

        public FtsModLocationControl() {
            InitializeComponent();

            FtsModLocationControlLocationTextboxControl.Directory = true;

            AddHandler(LocationTextboxControl.LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT, new RoutedEventHandler(FtsMainControlLocation_Update));
        }

        public void Show() {
            Visibility = Visibility.Visible;
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            if (string.IsNullOrEmpty(FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                string path = Path.GetDirectoryName((string) GameHandler.GetGameInstallation());
                if (!string.IsNullOrEmpty(path)) {
                    FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text = path;
                    LogHandler.LogInfo("Using Arma 3 location: " + FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text);
                }
            }

            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text != "" ? DESCRIPTION : DESCRIPTION_NOINSTALL
            });
            UpdateWarning();
        }

        public void Hide() => Visibility = Visibility.Collapsed;

        private void UpdateWarning() {
            if (Visibility != Visibility.Visible) return;
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Please select a mod download location";
                block = true;
            } else if (!GameHandler.CheckDriveSpace(FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Not enough drive space";
                block = true;
            }

            RaiseEvent(new SafeWindow.WarningRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_WARNING_EVENT) {Warning = warning, Block = block});
        }

        private void FtsMainControlLocation_Update(object sender, RoutedEventArgs args) {
            UpdateWarning();
        }
    }
}
