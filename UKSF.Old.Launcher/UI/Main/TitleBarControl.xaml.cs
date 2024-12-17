using System.Windows;
using System.Windows.Input;

namespace UKSF.Old.Launcher.UI.Main {
    public partial class TitleBarControl {
        public static readonly RoutedEvent MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TitleBarControl));
        public static readonly RoutedEvent MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TitleBarControl));

        public TitleBarControl() {
            InitializeComponent();
        }

        private void TitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT));
            }
        }

        private void TitleBarControlButtonClose_Click(object sender, RoutedEventArgs args) => Core.ShutDown();

        private void TitleBarControlButtonMinimize_Click(object sender, RoutedEventArgs args) =>
            RaiseEvent(new RoutedEventArgs(MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT));

        public void TitleBarControlButtonSettings_Click(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                MainWindow.Instance.HomeControl.Visibility = MainWindow.Instance.HomeControl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                MainWindow.Instance.SettingsControl.Visibility = MainWindow.Instance.SettingsControl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            });
        }
    }
}
