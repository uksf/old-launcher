using System.Windows;
using System.Windows.Controls;
using UKSF.Old.Launcher.Game;

namespace UKSF.Old.Launcher.UI.General {
    public class ProfileComboBoxItem : ComboBoxItem {
        public ProfileComboBoxItem(ProfileHandler.Profile profile, Style style) {
            Profile = profile;
            Content = Profile.DisplayName;
            Style = style;
        }

        public ProfileHandler.Profile Profile { get; }
    }
}
