using System.Windows;
using System.Windows.Input;

namespace UKSF.Old.Launcher.UI.Dialog {
    public partial class DialogTitleBarControl {
        public static readonly RoutedEvent DIALOG_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT =
            EventManager.RegisterRoutedEvent("DIALOG_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DialogTitleBarControl));

        public DialogTitleBarControl() {
            InitializeComponent();
        }

        private void DialogTitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(DIALOG_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT));
            }
        }

        private void DialogTitleBarControlButtonClose_Click(object sender, RoutedEventArgs args) {
            Core.ShutDown();
        }
    }
}
