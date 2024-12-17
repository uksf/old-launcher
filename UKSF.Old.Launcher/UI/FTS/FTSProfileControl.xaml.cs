using System.Windows;
using UKSF.Old.Launcher.UI.General;

namespace UKSF.Old.Launcher.UI.FTS {
    public partial class FtsProfileControl {
        private const string TITLE = "Game Profile";

        private static readonly string DESCRIPTION =
            $"We have selected the Arma 3 profile we think you use for UKSF.{Global.Constants.NL}If this is incorrect, select the profile you wish to use from the list below.{Global.Constants.NL}Alternatively, select a profile from the list below and press 'Copy' to create a new profile and copy your game settings from the selected profile.";

        private static readonly string DESCRIPTION_NOPROFILE =
            $"We can't find an Arma 3 profile suitable for UKSF.{Global.Constants.NL}Select a profile from the list below and press 'Copy' to create a new profile and copy your game settings from the selected profile.{Global.Constants.NL}You may skip this step if you do not wish to create a new profile. (Not recommended)";

        public FtsProfileControl() {
            AddHandler(ProfileSelectionControl.PROFILE_SELECTION_CONTROL_UPDATE_EVENT, new RoutedEventHandler(FtsProfileControlProfile_Update));

            InitializeComponent();
        }

        public void Show() {
            if (Visibility == Visibility.Visible) return;
            Visibility = Visibility.Visible;
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = FtsProfileControlProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem != null ? DESCRIPTION : DESCRIPTION_NOPROFILE
            });
            UpdateWarning();
        }

        public void Hide() => Visibility = Visibility.Collapsed;

        private void UpdateWarning() {
            if (Visibility != Visibility.Visible) return;
            string warning = "";
            bool block = false;
            if (FtsProfileControlProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedIndex == -1) {
                warning = "Please select a profile";
                block = true;
            }

            RaiseEvent(new SafeWindow.WarningRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_WARNING_EVENT) {Warning = warning, Block = block});
        }

        private void FtsProfileControlProfile_Update(object sender, RoutedEventArgs args) {
            UpdateWarning();
        }
    }
}
