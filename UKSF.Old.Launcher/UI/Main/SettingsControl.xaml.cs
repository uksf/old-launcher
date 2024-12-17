using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UKSF.Old.Launcher.Game;
using UKSF.Old.Launcher.UI.Dialog;
using UKSF.Old.Launcher.UI.General;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher.UI.Main {
    public partial class SettingsControl {
        public static readonly RoutedEvent MAIN_SETTINGS_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_SETTINGS_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SettingsControl));

        private Task _repoMoveTask;

        public SettingsControl() {
            InitializeComponent();

            AddHandler(MAIN_SETTINGS_CONTROL_WARNING_EVENT, new RoutedEventHandler(SettingsControlLocation_Update));
            AddHandler(MAIN_SETTINGS_CONTROL_WARNING_EVENT, new RoutedEventHandler(SettingsControlProfile_Update));
            AddHandler(LocationTextboxControl.LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT, new RoutedEventHandler(SettingsControlLocation_Update));
            AddHandler(ProfileSelectionControl.PROFILE_SELECTION_CONTROL_UPDATE_EVENT, new RoutedEventHandler(SettingsControlProfile_Update));
        }

        public void Initialise() {
            SettingsControlVersion.Content = "Version: " + Global.VERSION;
            SettingsControlCheckboxAutoupdateLauncher.IsChecked = Global.Settings.AutoUpdateLauncher;

            SettingsControlGameExeTextboxControl.Filter = "exe files|*.exe";
            SettingsControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text = Global.Settings.GameLocation;
            SettingsControlDownloadTextboxControl.Directory = true;
            SettingsControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text = Global.Settings.ModLocation;

            SettingsControlCheckboxSplash.IsChecked = Global.Settings.StartupNoSplash;
            SettingsControlCheckboxScript.IsChecked = Global.Settings.StartupScriptErrors;
            SettingsControlCheckboxPages.IsChecked = Global.Settings.StartupHugePages;

            SettingsControlDevControl.IsEnabled = false;
            SettingsControlCheckboxPatching.IsChecked = false;

            if (Global.Settings.Admin) {
                SettingsControlDevControl.IsEnabled = true;
                SettingsControlCheckboxPatching.IsChecked = Global.Settings.StartupFilePatching;
            }

            if (SettingsGameControlDropdownMalloc.Items.IsEmpty) {
                AddMallocs();
            }

            UpdateWarning();
            UpdateGameExeWarning();
            UpdateDownloadWarning();
        }

        private async void SettingsControlCheckBoxAutoupdateLauncher_Click(object sender, RoutedEventArgs args) {
            Global.Settings.AutoUpdateLauncher = (bool) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.AutoUpdateLauncher), SettingsControlCheckboxAutoupdateLauncher.IsChecked);
            if (Global.Settings.AutoUpdateLauncher) {
                await UpdateHandler.CheckVersion();
            }
        }

        private void SettingsControlLocation_Update(object sender, RoutedEventArgs args) {
            if (Global.Settings.GameLocation != SettingsControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text) {
                Global.Settings.GameLocation =
                    (string) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.GameLocation), SettingsControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text);
            }

            if (Global.Settings.ModLocation != SettingsControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text) {
                if (Global.GameProcess == null) {
                    if (_repoMoveTask == null) {
                        string newLocation = SettingsControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text;
                        if (!GameHandler.CheckDriveSpace(SettingsControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                            DialogWindow.Show("Drive Space",
                                              $"Not enough drive space at '{newLocation}'.\n\nPlease allow {Global.Constants.REQUIREDSPACE} of space.",
                                              DialogWindow.DialogBoxType.OK);
                        } else {
                            MessageBoxResult result = DialogWindow.Show("Move repo?",
                                                                        $"Are you sure you want to move the repo mods from\n\n'{Global.Settings.ModLocation}'\n to\n'{newLocation}'\n\n\nSelect 'No' to change the repo location without moving mod files",
                                                                        DialogWindow.DialogBoxType.YES_NO_CANCEL);
                            if (result != MessageBoxResult.Cancel) {
                                bool moveRepo = result != MessageBoxResult.No;
                                Core.CancellationTokenSource = new CancellationTokenSource();
                                Task.Run(() => {
                                    _repoMoveTask = Task.Run(() => {
                                                                 MainWindow.Instance.TitleBarControl.TitleBarControlButtonSettings_Click(this, new RoutedEventArgs());
                                                                 MainWindow.Instance.HomeControl
                                                                           .RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = false});
                                                                 MainWindow.Instance.HomeControl
                                                                           .RaiseEvent(new SafeWindow.IntRoutedEventArgs(HomeControl.HOME_CONTROL_STATE_EVENT) {Value = 1});
                                                                 string finalPath = Global.Repo.MoveRepo(newLocation, moveRepo, Core.CancellationTokenSource.Token);
                                                                 ServerHandler.SendServerMessage("reporequest uksf");
                                                                 MainWindow.Instance.HomeControl
                                                                           .RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = true});
                                                                 Global.Settings.ModLocation = (string) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.ModLocation), finalPath);
                                                                 Dispatcher.Invoke(() => SettingsControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text =
                                                                                             Global.Settings.ModLocation);
                                                             },
                                                             Core.CancellationTokenSource.Token);
                                    _repoMoveTask.Wait();
                                    _repoMoveTask = null;
                                });
                            }
                        }
                    }
                }
            }

            UpdateGameExeWarning();
            UpdateDownloadWarning();
        }

        private void UpdateGameExeWarning() {
            string warning = "";
            bool block = false;
            string path = SettingsControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text;
            if (string.IsNullOrEmpty(path)) {
                warning = "Please select an Arma 3 exe";
                block = true;
            } else if (Path.GetExtension(path) != ".exe") {
                warning = "File is not an exe";
                block = true;
            } else if (!Path.GetFileNameWithoutExtension(path).ToLower().Contains("arma3")) {
                warning = "File is not an Arma 3 exe";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(path).ToLower().Contains("battleye")) {
                warning = "Exe cannot be battleye";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(path).ToLower().Contains("launcher")) {
                warning = "Exe cannot be launcher";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(path).ToLower().Contains("server")) {
                warning = "Exe cannot be server";
                block = true;
            } else if (Global.Constants.IS64_BIT && !Path.GetFileNameWithoutExtension(path).ToLower().Contains("x64")) {
                warning = "We recommend using the 'arma3_x64' exe";
            }

            SettingsControlGameExeWarningText.Text = warning;
            if (MainWindow.Instance.HomeControl == null) return;
            MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.WarningRoutedEventArgs(HomeControl.HOME_CONTROL_WARNING_EVENT) {
                Warning = warning, Block = block, CurrentWarning = HomeControl.CurrentWarning.GAME_LOCATION
            });
        }

        private void UpdateDownloadWarning() {
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(SettingsControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Please select a mod download location";
                block = true;
            } else if (!GameHandler.CheckDriveSpace(SettingsControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Not enough drive space";
                block = true;
            }

            SettingsControlDownloadWarningText.Text = warning;
            if (MainWindow.Instance.HomeControl == null) return;
            MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.WarningRoutedEventArgs(HomeControl.HOME_CONTROL_WARNING_EVENT) {
                Warning = warning, Block = block, CurrentWarning = HomeControl.CurrentWarning.MOD_LOCATION
            });
        }

        private void AddMallocs() {
            SettingsGameControlDropdownMalloc.Items.Clear();
            List<MallocHandler.Malloc> mallocs = MallocHandler.GetMallocs();
            foreach (MallocHandler.Malloc malloc in mallocs) {
                SettingsGameControlDropdownMalloc.Items.Add(new MallocComboBoxItem(malloc, FindResource("Uksf.ComboBoxItem") as Style));

                if (malloc.Name == Global.Settings.StartupMalloc) {
                    SettingsGameControlDropdownMalloc.SelectedIndex = mallocs.IndexOf(malloc);
                }
            }

            if (SettingsGameControlDropdownMalloc.SelectedIndex > -1) return;
            SettingsGameControlDropdownMalloc.SelectedIndex = 0;
        }

        private void SettingsControlProfile_Update(object sender, RoutedEventArgs args) {
            if (SettingsControlDropdownProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem != null) {
                ProfileComboBoxItem selectedProfile = (ProfileComboBoxItem) SettingsControlDropdownProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem;
                if (Global.Settings.Profile != selectedProfile.Profile.DisplayName) {
                    Global.Settings.Profile = (string) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.Profile), selectedProfile.Profile.DisplayName);
                }
            }

            UpdateWarning();
        }

        private void UpdateWarning() {
            string warning = "";
            bool block = false;
            if (SettingsControlDropdownProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedIndex == -1) {
                warning = "Please select a profile";
                block = true;
            }

            SettingsControlProfileWarningText.Text = warning;
            if (MainWindow.Instance.HomeControl == null) return;
            MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.WarningRoutedEventArgs(HomeControl.HOME_CONTROL_WARNING_EVENT) {
                Warning = warning, Block = block, CurrentWarning = HomeControl.CurrentWarning.PROFILE
            });
        }

        private void SettingsLauncherControlMalloc_Update(object sender, RoutedEventArgs args) {
            if (SettingsGameControlDropdownMalloc.SelectedIndex == -1 ||
                Global.Settings.StartupMalloc == ((MallocComboBoxItem) SettingsGameControlDropdownMalloc.SelectedItem).Malloc.Name) {
                return;
            }

            Global.Settings.StartupMalloc =
                (string) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.StartupMalloc), ((MallocComboBoxItem) SettingsGameControlDropdownMalloc.SelectedItem).Malloc.Name);
            if (Equals(((MallocComboBoxItem) SettingsGameControlDropdownMalloc.SelectedItem).Malloc.Name, Global.Constants.MALLOC_SYSTEM_DEFAULT)) return;
            SettingsControlCheckboxPages.IsChecked = false;
            Global.Settings.StartupHugePages = (bool) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.StartupHugePages), false);
        }

        private void SettingsControlCheckbox_Click(object sender, RoutedEventArgs args) {
            if (Equals(sender, SettingsControlCheckboxSplash)) {
                Global.Settings.StartupNoSplash = (bool) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.StartupNoSplash), SettingsControlCheckboxSplash.IsChecked);
            } else if (Equals(sender, SettingsControlCheckboxScript)) {
                Global.Settings.StartupScriptErrors = (bool) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.StartupScriptErrors), SettingsControlCheckboxScript.IsChecked);
            } else if (Equals(sender, SettingsControlCheckboxPatching)) {
                Global.Settings.StartupFilePatching = (bool) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.StartupFilePatching), SettingsControlCheckboxPatching.IsChecked);
            } else if (Equals(sender, SettingsControlCheckboxPages)) {
                Global.Settings.StartupHugePages = (bool) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.StartupHugePages), SettingsControlCheckboxPages.IsChecked);
                SettingsGameControlDropdownMalloc.SelectedIndex = 0;
            }
        }
    }
}
