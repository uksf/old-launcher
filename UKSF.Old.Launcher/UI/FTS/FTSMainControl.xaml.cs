using System.Windows;
using UKSF.Old.Launcher.UI.General;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher.UI.FTS {
    public partial class FtsMainControl {
        public static readonly RoutedEvent FTS_MAIN_CONTROL_DESCRIPTION_EVENT =
            EventManager.RegisterRoutedEvent("FTS_MAIN_CONTROL_DESCRIPTION_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsMainControl));

        public static readonly RoutedEvent FTS_MAIN_CONTROL_FINISH_EVENT =
            EventManager.RegisterRoutedEvent("FTS_MAIN_CONTROL_FINISH_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsMainControl));
        public static readonly RoutedEvent FTS_MAIN_CONTROL_TITLE_EVENT =
            EventManager.RegisterRoutedEvent("FTS_MAIN_CONTROL_TITLE_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsMainControl));

        public static readonly RoutedEvent FTS_MAIN_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("FTS_MAIN_CONTROL_WARNING_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsMainControl));

        private int _windowIndex;

        public FtsMainControl() {
            AddHandler(FTS_MAIN_CONTROL_TITLE_EVENT, new RoutedEventHandler(FTSMainControlTitle_Update));
            AddHandler(FTS_MAIN_CONTROL_DESCRIPTION_EVENT, new RoutedEventHandler(FTSMainControlDescription_Update));
            AddHandler(FTS_MAIN_CONTROL_WARNING_EVENT, new RoutedEventHandler(FTSMainControlWarning_Update));

            InitializeComponent();

            UpdateControls();
        }

        private void FTSMainControlTitle_Update(object sender, RoutedEventArgs args) {
            SafeWindow.StringRoutedEventArgs stringArgs = (SafeWindow.StringRoutedEventArgs) args;
            FtsMainControlTitle.Content = stringArgs.Text;
        }

        private void FTSMainControlDescription_Update(object sender, RoutedEventArgs args) {
            SafeWindow.StringRoutedEventArgs stringArgs = (SafeWindow.StringRoutedEventArgs) args;
            FtsMainControlDescription.Text = stringArgs.Text;
        }

        private void FTSMainControlWarning_Update(object sender, RoutedEventArgs args) {
            SafeWindow.WarningRoutedEventArgs warningArgs = (SafeWindow.WarningRoutedEventArgs) args;
            FtsMainControlWarningText.Text = warningArgs.Warning;
            if (warningArgs.Block) {
                FtsMainControlButtonNext.IsEnabled = false;
                FtsMainControlButtonFinish.IsEnabled = false;
            } else {
                FtsMainControlButtonNext.IsEnabled = true;
                FtsMainControlButtonFinish.IsEnabled = true;
            }
        }

        private void FTSMainControlButtonProgress_Click(object sender, RoutedEventArgs args) {
            if (Equals(sender, FtsMainControlButtonNext)) {
                _windowIndex++;
            } else {
                _windowIndex--;
            }

            UpdateControls();
        }

        private void UpdateControls() {
            switch (_windowIndex) {
                case 0:
                    FtsGameExeControl.Show();
                    FtsModLocationControl.Hide();
                    FtsProfileControl.Hide();

                    FtsMainControlButtonNext.Visibility = Visibility.Visible;
                    FtsMainControlButtonBack.Visibility = Visibility.Collapsed;
                    FtsMainControlButtonFinish.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    FtsGameExeControl.Hide();
                    FtsModLocationControl.Show();
                    FtsProfileControl.Hide();

                    FtsMainControlButtonNext.Visibility = Visibility.Visible;
                    FtsMainControlButtonBack.Visibility = Visibility.Visible;
                    FtsMainControlButtonFinish.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    FtsGameExeControl.Hide();
                    FtsModLocationControl.Hide();
                    FtsProfileControl.Show();

                    FtsMainControlButtonNext.Visibility = Visibility.Collapsed;
                    FtsMainControlButtonBack.Visibility = Visibility.Visible;
                    FtsMainControlButtonFinish.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void FTSMainControlButtonFinish_Click(object sender, RoutedEventArgs args) {
            LogHandler.LogInfo("Finishing first time setup");
            Global.Settings.GameLocation = (string) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.GameLocation),
                                                                                      FtsGameExeControl.FtsGameExeControlLocationTextboxControl
                                                                                                       .LocationTextboxControlTextBoxLocation.Text);
            Global.Settings.ModLocation = (string) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.ModLocation),
                                                                                     FtsModLocationControl
                                                                                         .FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text);
            Global.Settings.Profile = (string) Core.SettingsHandler.WriteSetting(nameof(Global.Settings.Profile),
                                                                                 ((ProfileComboBoxItem) FtsProfileControl
                                                                                                        .FtsProfileControlProfileSelectionControl
                                                                                                        .ProfileSelectionControlDropdownProfile.SelectedItem).Profile.DisplayName);
            RaiseEvent(new RoutedEventArgs(FTS_MAIN_CONTROL_FINISH_EVENT));
        }

        private void FTSMainControlButtonCancel_Click(object sender, RoutedEventArgs args) {
            LogHandler.LogSeverity(Global.Severity.WARNING, "First Time Setup Cancelled. Progress has not been saved.");
            Core.ShutDown();
        }
    }
}
