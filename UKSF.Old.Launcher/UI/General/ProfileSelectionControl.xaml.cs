using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UKSF.Old.Launcher.Game;

namespace UKSF.Old.Launcher.UI.General {
    public partial class ProfileSelectionControl {
        public static readonly RoutedEvent PROFILE_SELECTION_CONTROL_UPDATE_EVENT =
            EventManager.RegisterRoutedEvent("PROFILE_SELECTION_CONTROL_UPDATE_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ProfileSelectionControl));

        public ProfileSelectionControl() {
            InitializeComponent();

            if (ProfileSelectionControlDropdownProfile.Items.IsEmpty) {
                AddProfiles();
            }
        }

        private void AddProfiles() {
            ProfileSelectionControlDropdownProfile.Items.Clear();
            List<ProfileHandler.Profile> profiles = ProfileHandler.Profiles;
            foreach (ProfileHandler.Profile profile in profiles) {
                ProfileComboBoxItem item = new ProfileComboBoxItem(profile, FindResource("Uksf.ComboBoxItem") as Style);
                ProfileSelectionControlDropdownProfile.Items.Add(item);

                if (!Global.Settings.FirstTimeSetupDone || profile.DisplayName != Global.Settings.Profile) continue;
                ProfileSelectionControlDropdownProfile.SelectedIndex = profiles.IndexOf(profile);
                ProfileHandler.CheckProfileName(profile);
                ((ProfileComboBoxItem) ProfileSelectionControlDropdownProfile.SelectedItem).Content = profile.DisplayName;
            }

            if (ProfileSelectionControlDropdownProfile.SelectedIndex == -1) {
                ProfileHandler.Profile profile = ProfileHandler.FindFormattedProfile(profiles);
                if (profile != null && (!Global.Settings.FirstTimeSetupDone || Global.Settings.FirstTimeSetupDone && Global.Settings.Profile == "")) {
                    ProfileSelectionControlDropdownProfile.SelectedIndex = profiles.IndexOf(profile);
                }
            }

            RaiseEvent(new RoutedEventArgs(PROFILE_SELECTION_CONTROL_UPDATE_EVENT));
        }

        private void ProfileSelectionControlDropdownProfile_Selected(object sender, SelectionChangedEventArgs args) {
            if (ProfileSelectionControlDropdownProfile.SelectedIndex <= -1) return;
            ProfileComboBoxItem selectedProfile = (ProfileComboBoxItem) ProfileSelectionControlDropdownProfile.SelectedItem;
            if (ProfileSelectionControlDropdownProfile.Items.Cast<ProfileComboBoxItem>().All(x => x.Profile.DisplayName != Global.Settings.AccountName)) {
                int result = ProfileHandler.CheckProfileName(selectedProfile.Profile);
                if (result == 3) {
                    ProfileComboBoxItem item = ProfileSelectionControlDropdownProfile
                                               .Items.Cast<ProfileComboBoxItem>()
                                               .FirstOrDefault(x => x.Profile.DisplayName == Global.Settings.AccountName);
                    if (item != null) {
                        ProfileSelectionControlDropdownProfile.SelectedItem = item;
                    }
                } else if (result == 0) {
                    selectedProfile.Content = selectedProfile.Profile.DisplayName;
                }
            }

            ProfileSelectionControlButtonCopy.Visibility = ProfileSelectionControlDropdownProfile
                                                           .Items.Cast<ProfileComboBoxItem>()
                                                           .Any(x => x.Profile.DisplayName == Global.Settings.AccountName)
                                                               ? Visibility.Collapsed
                                                               : Visibility.Visible;
            RaiseEvent(new RoutedEventArgs(PROFILE_SELECTION_CONTROL_UPDATE_EVENT));
        }

        private void ProfileSelectionControlButtonCopy_Click(object sender, RoutedEventArgs args) {
            ProfileHandler.CopyProfile(((ProfileComboBoxItem) ProfileSelectionControlDropdownProfile.SelectedItem).Profile, Global.Constants.PROFILE_LOCATION_OTHER);
            AddProfiles();
            ProfileComboBoxItem item = ProfileSelectionControlDropdownProfile
                                       .Items.Cast<ProfileComboBoxItem>()
                                       .FirstOrDefault(x => x.Profile.DisplayName == Global.Settings.AccountName);
            if (item != null) {
                ProfileSelectionControlDropdownProfile.SelectedItem = item;
            }
        }
    }
}
