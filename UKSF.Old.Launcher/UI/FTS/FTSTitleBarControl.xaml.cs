using System.Windows;
using System.Windows.Input;

namespace UKSF.Old.Launcher.UI.FTS {
    public partial class FtsTitleBarControl {
        public static readonly RoutedEvent FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT =
            EventManager.RegisterRoutedEvent("FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsTitleBarControl));

        public FtsTitleBarControl() {
            InitializeComponent();
        }

        private void FTSTitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT));
            }
        }

        private void FTSTitleBarControlButtonClose_Click(object sender, RoutedEventArgs args) => Core.ShutDown();
    }
}
