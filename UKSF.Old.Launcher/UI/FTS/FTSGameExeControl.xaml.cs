using System.IO;
using System.Windows;
using UKSF.Old.Launcher.Game;
using UKSF.Old.Launcher.UI.General;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher.UI.FTS {
    public partial class FtsGameExeControl {
        private const string TITLE = "Game Executable";

        private static readonly string DESCRIPTION = "We found your Arma 3 installation and chose the best exe for you." + Global.Constants.NL +
                                                     "If you are not happy with this, select the Arma 3 exe you wish to use.";

        private static readonly string DESCRIPTION_NOINSTALL = "We can't find your Arma 3 installation." + Global.Constants.NL +
                                                               "This is unusual, so you should check the game is installed in Steam." + Global.Constants.NL +
                                                               "You can continue by selecting the Arma 3 exe you wish to use manually. (Not recommended)";

        public FtsGameExeControl() {
            InitializeComponent();

            FtsGameExeControlLocationTextboxControl.Filter = "exe files|*.exe";

            AddHandler(LocationTextboxControl.LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT, new RoutedEventHandler(FtsGameExeControlLocation_Update));
        }

        public void Show() {
            Visibility = Visibility.Visible;
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            if (string.IsNullOrEmpty(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text = GameHandler.GetGameInstallation();
                if (!string.IsNullOrEmpty(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                    LogHandler.LogInfo("Using detected Arma 3 location: " + FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text);
                }
            }

            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text != "" ? DESCRIPTION : DESCRIPTION_NOINSTALL
            });
            UpdateWarning();
        }

        public void Hide() => Visibility = Visibility.Collapsed;

        private void UpdateWarning() {
            if (Visibility != Visibility.Visible) return;
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Please select an Arma 3 exe";
                block = true;
            } else if (Path.GetExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text) != ".exe") {
                warning = "File is not an exe";
                block = true;
            } else if (!Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("arma3")) {
                warning = "File is not an Arma 3 exe";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("battleye")) {
                warning = "Exe cannot be battleye";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("launcher")) {
                warning = "Exe cannot be launcher";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("server")) {
                warning = "Exe cannot be server";
                block = true;
            } else if (Global.Constants.IS64_BIT && !Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)
                                                         .ToLower()
                                                         .Contains("x64")) {
                warning = "We recommend using the 'arma3_x64' exe";
            }

            RaiseEvent(new SafeWindow.WarningRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_WARNING_EVENT) {Warning = warning, Block = block});
        }

        private void FtsGameExeControlLocation_Update(object sender, RoutedEventArgs args) {
            UpdateWarning();
        }
    }
}
