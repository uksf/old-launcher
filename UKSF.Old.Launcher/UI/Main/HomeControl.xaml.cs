using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UKSF.Launcher.Network;
using UKSF.Old.Launcher.Game;
using UKSF.Old.Launcher.UI.General;

namespace UKSF.Old.Launcher.UI.Main {
    public partial class HomeControl {
        public enum CurrentWarning {
            NONE,
            GAME_LOCATION,
            MOD_LOCATION,
            PROFILE
        }

        public static readonly RoutedEvent HOME_CONTROL_PLAY_EVENT =
            EventManager.RegisterRoutedEvent("HOME_CONTROL_PLAY_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(HomeControl));

        public static readonly RoutedEvent HOME_CONTROL_PROGRESS_EVENT =
            EventManager.RegisterRoutedEvent("HOME_CONTROL_PROGRESS_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(HomeControl));

        public static readonly RoutedEvent HOME_CONTROL_SERVER_EVENT =
            EventManager.RegisterRoutedEvent("HOME_CONTROL_SERVER_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(HomeControl));

        public static readonly RoutedEvent HOME_CONTROL_STATE_EVENT =
            EventManager.RegisterRoutedEvent("HOME_CONTROL_STATE_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(HomeControl));

        public static readonly RoutedEvent HOME_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("HOME_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(HomeControl));

        private bool _block = true;

        private CurrentWarning _currentWarning = CurrentWarning.NONE;

        public HomeControl() {
            InitializeComponent();

            AddHandler(HOME_CONTROL_PLAY_EVENT, new RoutedEventHandler(HomeControlPlay_Update));
            AddHandler(HOME_CONTROL_PROGRESS_EVENT, new RoutedEventHandler(HomeControlProgress_Update));
            AddHandler(HOME_CONTROL_WARNING_EVENT, new RoutedEventHandler(HomeControlWarning_Update));
            AddHandler(HOME_CONTROL_SERVER_EVENT, new RoutedEventHandler(HomeControlServer_Update));
            AddHandler(HOME_CONTROL_STATE_EVENT, new RoutedEventHandler(HomeControlState_Update));

            HomeControlProgressBar.Visibility = Visibility.Collapsed;
            HomeControlProgressText.Visibility = Visibility.Collapsed;
            HomeControlDropdownServer.Visibility = Visibility.Collapsed;
            HomeControlState_Update(null, new SafeWindow.IntRoutedEventArgs(HOME_CONTROL_STATE_EVENT) {Value = -1});
            // TODO: Sims-esque loading messages
            // TODO: Implement background workers for all non-ui code, using progresschanged event for updating ui
        }

        private void HomeControlPlay_Update(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                SafeWindow.BoolRoutedEventArgs boolArgs = (SafeWindow.BoolRoutedEventArgs) args;
                if (_block && !string.Equals(HomeControlWarningText.Text, "", StringComparison.InvariantCultureIgnoreCase)) return;
                HomeControlButtonPlay.IsEnabled = boolArgs.State;
                HomeControlDropdownServer.IsEnabled = boolArgs.State;
            });
        }

        private void HomeControlProgress_Update(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                SafeWindow.ProgressRoutedEventArgs progressArgs = (SafeWindow.ProgressRoutedEventArgs) args;
                if (!progressArgs.Message.Contains("stop")) {
                    HomeControlProgressBar.Visibility = Visibility.Visible;
                    HomeControlProgressText.Visibility = Visibility.Visible;
                    HomeControlProgressBar.Value = progressArgs.Value;
                    HomeControlProgressText.Text = progressArgs.Message;
                } else {
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HOME_CONTROL_PLAY_EVENT) {State = true});
                    HomeControlProgressBar.Visibility = Visibility.Collapsed;
                    HomeControlProgressText.Visibility = Visibility.Collapsed;
                }
            });
        }

        private void HomeControlWarning_Update(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                SafeWindow.WarningRoutedEventArgs warningArgs = (SafeWindow.WarningRoutedEventArgs) args;
                CurrentWarning previousWarning = _currentWarning;
                if (_currentWarning != CurrentWarning.NONE && _currentWarning != warningArgs.CurrentWarning) return;
                _block = warningArgs.Block;
                HomeControlWarningText.Text = warningArgs.Warning;
                MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HOME_CONTROL_PLAY_EVENT) {State = !_block});
                _currentWarning = warningArgs.Warning == "" ? CurrentWarning.NONE : warningArgs.CurrentWarning;
                if (_currentWarning != previousWarning) {
                    MainWindow.Instance.SettingsControl.RaiseEvent(new RoutedEventArgs(SettingsControl.MAIN_SETTINGS_CONTROL_WARNING_EVENT));
                }
            });
        }

        private void HomeControlServer_Update(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                SafeWindow.ServerRoutedEventArgs serverArgs = (SafeWindow.ServerRoutedEventArgs) args;
                List<Server> servers = Enumerable.Where(serverArgs.Servers, server => server.Active).ToList();
                if (servers.Count > 0) {
                    HomeControlDropdownServer.Visibility = Visibility.Visible;
                    HomeControlDropdownServer.Items.Clear();
                    HomeControlDropdownServer.Items.Add(new ServerComboBoxItem(ServerHandler.NO_SERVER, FindResource("Uksf.ComboBoxItemPlay") as Style));
                    foreach (ServerComboBoxItem serverComboBoxItem in servers.Select(server => new ServerComboBoxItem(server, FindResource("Uksf.ComboBoxItemPlay") as Style))) {
                        HomeControlDropdownServer.Items.Add(serverComboBoxItem);
                        if (Global.Server != null && serverComboBoxItem.Server.Name == Global.Server.Name) {
                            HomeControlDropdownServer.SelectedItem = serverComboBoxItem;
                        }
                    }
                } else {
                    if (HomeControlDropdownServer.Items.Count > 0) HomeControlDropdownServer.SelectedItem = HomeControlDropdownServer.Items.GetItemAt(0);
                    HomeControlDropdownServer.Visibility = Visibility.Collapsed;
                    Global.Server = null;
                }

                HomeControlDropdownServer_Selected(null, null);
            });
        }

        private void HomeControlState_Update(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                SafeWindow.IntRoutedEventArgs stateArgs = (SafeWindow.IntRoutedEventArgs) args;
                switch (stateArgs.Value) {
                    case -1:
                        HomeControlControllerButton.Content = "";
                        HomeControlControllerButton.IsEnabled = false;
                        break;
                    case 1:
                        HomeControlControllerButton.Content = "Cancel";
                        HomeControlControllerButton.IsEnabled = true;
                        break;
                    case 2:
                        HomeControlControllerButton.Content = "Kill Game";
                        HomeControlControllerButton.IsEnabled = true;
                        break;
                    default:
                        HomeControlControllerButton.Content = "Refresh";
                        HomeControlControllerButton.IsEnabled = true;
                        break;
                }
            });
        }

        private void HomeControlButtonPlay_Click(object sender, RoutedEventArgs args) {
            GameHandler.StartGame();
        }

        private void HomeControlDropdownServer_Selected(object sender, SelectionChangedEventArgs args) {
            if (HomeControlDropdownServer.SelectedItem != null) {
                Global.Server = ((ServerComboBoxItem) HomeControlDropdownServer.SelectedItem).Server;
                if (Equals(Global.Server, ServerHandler.NO_SERVER)) {
                    HomeControlButtonPlay.Content = "Play";
                    HomeControlButtonPlay.FontSize = 50;
                } else {
                    HomeControlButtonPlay.Content = Global.Server.Name;
                    HomeControlButtonPlay.FontSize = 30;
                }
            } else {
                HomeControlButtonPlay.Content = "Play";
                HomeControlButtonPlay.FontSize = 50;
            }
        }

        private async void HomeControlRefreshCancelButton_Click(object sender, RoutedEventArgs args) {
            HomeControlControllerButton.IsEnabled = false;
            if (Global.GameProcess == null) {
                if (HomeControlControllerButton.Content.Equals("Cancel")) {
                    Core.CancellationTokenSource.Cancel();
                    await Task.Delay(250);
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(HOME_CONTROL_PROGRESS_EVENT) {Value = 1, Message = "stop"});
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(HOME_CONTROL_STATE_EVENT) {Value = 0});
                } else if (HomeControlControllerButton.Content.Equals("Refresh")) {
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(HOME_CONTROL_STATE_EVENT) {Value = 1});
                    ServerHandler.SendServerMessage("reporequest uksf");
                }
            } else {
                Global.GameProcess.Kill();
                MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(HOME_CONTROL_STATE_EVENT) {Value = 0});
            }

            await Task.Delay(250);
            HomeControlControllerButton.IsEnabled = true;
        }
    }
}
